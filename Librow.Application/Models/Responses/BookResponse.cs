namespace Librow.Application.Models.Responses;
public class BookResponse
{
    public Guid Id { get; set; }
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; }
    public string Title { get; set; }
    public string Author { get; set; }
    public int Quantity { get; set; }
    public int Available { get; set; }
}
