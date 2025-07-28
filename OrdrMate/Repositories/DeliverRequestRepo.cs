namespace OrdrMate.Repositories;

using Microsoft.EntityFrameworkCore;
using OrdrMate.Data;
using OrdrMate.Models;

public class DeliverRequestRepo : IDeliverRequestRepo
{
    private readonly OrdrMateDbContext _context;

    public DeliverRequestRepo(OrdrMateDbContext context)
    {
        _context = context;
    }

    public async Task<DeliverRequest?> GetDeliverRequestById(string id)
    {
        return await _context.DeliverRequest.FindAsync(id);
    }

    public async Task<IEnumerable<DeliverRequest>> GetAllDeliverRequests()
    {
        return await _context.DeliverRequest.ToListAsync();
    }

    public async Task<DeliverRequest> AddDeliverRequest(DeliverRequest deliverRequest)
    {
        await _context.DeliverRequest.AddAsync(deliverRequest);
        await _context.SaveChangesAsync();
        return deliverRequest;
    }

    public async Task UpdateDeliverRequest(DeliverRequest deliverRequest)
    {
        _context.DeliverRequest.Update(deliverRequest);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteDeliverRequest(string id)
    {
        var deliverRequest = await GetDeliverRequestById(id);
        if (deliverRequest != null)
        {
            _context.DeliverRequest.Remove(deliverRequest);
            await _context.SaveChangesAsync();
        }
    }
}
