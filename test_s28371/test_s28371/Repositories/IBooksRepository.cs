using test_s28371.Models.DTOs;

namespace test_s28371.Repositories;

public interface IBooksRepository
{
    Task<bool> DoesBookExist(int id);
    Task<List<BookDTO>> GetBookEditions(int id);

    Task<bool> DoesTitleExist(string bookTitle);
    Task<bool> DoesPublishingHouseExist(string publishingHouseId);
    Task<BookDTO> AddNewBook(DataToAccept dataToAccept);
}