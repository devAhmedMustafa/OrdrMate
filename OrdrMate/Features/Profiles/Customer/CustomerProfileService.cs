namespace OrdrMate.Features.Profiles.Customer;

public class CustomerProfileService
{
    
    private readonly CustomerProfileRepo _repo;

    public CustomerProfileService(CustomerProfileRepo repo)
    {
        _repo = repo;
    }


}