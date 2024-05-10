using Microsoft.AspNetCore.Mvc;
using test_s28371.Models.DTOs;
using test_s28371.Repositories;

namespace test_s28371.Controllers;


[Route("api/[controller]")]
[ApiController]
public class BooksController : ControllerBase
{
    private readonly IBooksRepository _booksRepository;

    public BooksController(IBooksRepository booksRepository)
    {
        _booksRepository = booksRepository;
    }
    
    [HttpGet("{id}/editions")]
    public async Task<IActionResult> GetBookEditions(int id)
    {
        if (!await _booksRepository.DoesBookExist(id))
            return NotFound($"Book with given ID - {id} doesn't exist");

        var books = await _booksRepository.GetBookEditions(id);
            
        return Ok(books);
    }

    [HttpPost]
    public async Task<IActionResult> AddAsync(DataToAccept dataToAccept)
    {
        if (await _booksRepository.DoesTitleExist(dataToAccept.bookTitle))
            return BadRequest($"Book with given name - {dataToAccept.bookTitle} already exists");
        if (!await _booksRepository.DoesPublishingHouseExist(dataToAccept.publishingHouseId))
            return NotFound($"Publishing house with given ID - {dataToAccept.publishingHouseId} doesn't exist");
        try
        {
            var newBook = await _booksRepository.AddNewBook(dataToAccept);
            return Created(Request.Path.Value ?? "api/books", newBook);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return BadRequest(e.Message);
        }

       
        

    }
}