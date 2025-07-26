namespace OrdrMate.Repositories;

using OrdrMate.Models;

public interface IDeliverRequestRepo
{
    Task<DeliverRequest?> GetDeliverRequestById(string id);
    Task<IEnumerable<DeliverRequest>> GetAllDeliverRequests();
    Task<DeliverRequest> AddDeliverRequest(DeliverRequest deliverRequest);
    Task UpdateDeliverRequest(DeliverRequest deliverRequest);
    Task DeleteDeliverRequest(string id);
}
