using Twilio.Rest.Api.V2010.Account;

namespace Yabraa.IServices
{
    public interface ISMSService
    {
        Task<MessageResource> Send(string mobileNumber, string body);
    }
}
