using Microsoft.Data.SqlClient;
using test_s28371.Models.DTOs;

namespace test_s28371.Repositories;

public class BooksRepository : IBooksRepository
{
    private readonly IConfiguration _configuration;

    public BooksRepository(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    public async Task<bool> DoesBookExist(int id)
    {
        var query = "SELECT 1 FROM books WHERE PK = @ID";

        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand();

        command.Connection = connection;
        command.CommandText = query;
        command.Parameters.AddWithValue("@ID", id);

        await connection.OpenAsync();

        var res = await command.ExecuteScalarAsync();

        return res is not null;
    }

    public async Task<List<BookDTO>> GetBookEditions(int id)
    {
        var query = @"SELECT 
							books.PK AS id,
							books.title AS bookTitle,
							books_editions.editionTitle as editionTitle,
    						publishing_houses.name as publishingHouseName, 
    						books_editions.releaseDate as releaseDate
						FROM books
						JOIN books_editions ON books_editions.FK_book = books.PK
						JOIN publishing_houses ON publishing_houses.PK = books_editions.FK_publishing_house
						WHERE books.PK = @ID";
        
        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand();

        command.Connection = connection;
        command.CommandText = query;
        command.Parameters.AddWithValue("@ID", id);

        await connection.OpenAsync();

        var reader =  await command.ExecuteReaderAsync();
        
        var idOrdinal = reader.GetOrdinal("id");
        var bookTitleOrdinal = reader.GetOrdinal("bookTitle");
        var editionTitleOrdinal = reader.GetOrdinal("editionTitle");
        var publishingHouseNameOrdinal = reader.GetOrdinal("publishingHouseName");
        var releaseDateOrdinal = reader.GetOrdinal("releaseDate");

        List<BookDTO> editions = new List<BookDTO>();

        while (await reader.ReadAsync())
        {
	        editions.Add(new BookDTO()
	        {
		        id = reader.GetInt32(idOrdinal),
		        bookTitle = reader.GetString(bookTitleOrdinal),
		        editionTitle = reader.GetString(editionTitleOrdinal),
		        publishingHouseName = reader.GetString(publishingHouseNameOrdinal),
		        releaseDate = reader.GetDateTime(releaseDateOrdinal)
	        });
	        
        }

        return editions;
        
    }

    public async Task<bool> DoesTitleExist(string bookTitle)
    {
	    var query = "SELECT 1 FROM books WHERE title = @title";

	    await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
	    await using SqlCommand command = new SqlCommand();

	    command.Connection = connection;
	    command.CommandText = query;
	    command.Parameters.AddWithValue("@title", bookTitle);

	    await connection.OpenAsync();

	    var res = await command.ExecuteScalarAsync();

	    return res is not null;
    }

    public async Task<bool> DoesPublishingHouseExist(string publishingHouseId)
    {
	    var query = "SELECT 1 FROM publishing_houses WHERE PK = @ID";

	    await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
	    await using SqlCommand command = new SqlCommand();

	    command.Connection = connection;
	    command.CommandText = query;
	    command.Parameters.AddWithValue("@ID", publishingHouseId);

	    await connection.OpenAsync();

	    var res = await command.ExecuteScalarAsync();

	    return res is not null;
    }

    public async Task<BookDTO> AddNewBook(DataToAccept dataToAccept)
    {
	    var insert = @"INSERT INTO books VALUES(@title);
					   SELECT @@IDENTITY AS ID;";

	    await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
	    await using SqlCommand command = new SqlCommand();

	    command.Connection = connection;
	    command.CommandText = insert;
	    command.Parameters.AddWithValue("@title", dataToAccept.bookTitle);

	    await connection.OpenAsync();

	    var transaction = await connection.BeginTransactionAsync();
	    command.Transaction = transaction as SqlTransaction;
	    string bookid;
	    
	    try
	    {
		    var id = await command.ExecuteScalarAsync();
    
		     command.Parameters.Clear();
			    command.CommandText = "INSERT INTO books_editions VALUES (@publishingHouseId, @bookId, @editionTitle, @releaseDate)";
			    command.Parameters.AddWithValue("@publishingHouseId", dataToAccept.publishingHouseId);
			    command.Parameters.AddWithValue("@bookId",id );
			    command.Parameters.AddWithValue("@editionTitle", dataToAccept.editionTitle);
			    command.Parameters.AddWithValue("@releaseDate", dataToAccept.releaseDate);

			    await command.ExecuteNonQueryAsync();

			    bookid = id.ToString();
		    await transaction.CommitAsync();
	    }
	    catch (Exception)
	    {
		    await transaction.RollbackAsync();
		    throw;
	    }

	    return await GetOneEdition(int.Parse(bookid));
    }

    private async Task<BookDTO> GetOneEdition(int id)
    {
	    List<BookDTO> books =  await GetBookEditions(id);
	    return books[0];
    }
}