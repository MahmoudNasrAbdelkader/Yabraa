using Microsoft.Extensions.Options;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Yabraa.Helpers;
using Yabraa.IServices;

namespace Yabraa.Services
{
    public class SMSService : ISMSService
    {
        private readonly TwilioSettings _twilio;

        public SMSService(IOptions<TwilioSettings> twilio)
        {
            _twilio = twilio.Value;
        }

        public Task<MessageResource> Send(string mobileNumber, string body)
        {
            TwilioClient.Init(_twilio.AccountSID, _twilio.AuthToken);

            var result = MessageResource.CreateAsync(
                    body: body,
                    from: new Twilio.Types.PhoneNumber(_twilio.TwilioPhoneNumber),
                    to: mobileNumber
                );

            return  result;
        }
    }
}
