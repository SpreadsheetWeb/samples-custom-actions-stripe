using Pagos.Designer.Interfaces.External.CustomHooks;
using Pagos.Designer.Interfaces.External.Messaging;
using Pagos.SpreadsheetWeb.Web.Api.Objects.Calculation;
using Stripe;
using System;
using System.Linq;

namespace StripeExample
{
    public class StripeCalculator : IAfterCalculation
    {
        private string apiKey = "your_stripe_secret_key";

        public ActionableResponse AfterCalculation(CalculationRequest request, CalculationResponse response)
        {
            var name = request.Inputs.FirstOrDefault(x => x.Ref == "iName");
            var lastName = request.Inputs.FirstOrDefault(x => x.Ref == "iSurname");
            var cardNumber = request.Inputs.FirstOrDefault(x => x.Ref == "iCardNumber");
            var year = request.Inputs.FirstOrDefault(x => x.Ref == "iYear");
            var month = request.Inputs.FirstOrDefault(x => x.Ref == "iMonth");
            var cvc = request.Inputs.FirstOrDefault(x => x.Ref == "iCVC");
            var amount = request.Inputs.FirstOrDefault(x => x.Ref == "iAmount");
            var description = request.Inputs.FirstOrDefault(x => x.Ref == "iDesc");

            try
            {
                StripeConfiguration.SetApiKey(apiKey);

                var tokenOptions = new StripeTokenCreateOptions()
                {
                    Card = new StripeCreditCardOptions()
                    {
                        Name = name.Value[0][0].Value + " " + lastName.Value[0][0].Value,
                        Number = cardNumber.Value[0][0].Value.ToString().Replace(" ", ""),
                        ExpirationYear = Convert.ToInt32(year.Value[0][0].Value),
                        ExpirationMonth = Convert.ToInt32(month.Value[0][0].Value),
                        Cvc = cvc.Value[0][0].Value
                    }
                };

                var tokenService = new StripeTokenService();
                StripeToken stripeToken = tokenService.Create(tokenOptions);
                var myCharge = new StripeChargeCreateOptions
                {
                    Amount = Convert.ToInt32(amount.Value[0][0].Value),
                    Currency = "gbp",
                    Description = description.Value[0][0].Value,
                    SourceTokenOrExistingSourceId = stripeToken.Id
                };

                var chargeService = new StripeChargeService();

                var stripeCharge = chargeService.Create(myCharge);
                System.Diagnostics.Debug.WriteLine(stripeCharge.Id);
                response.Outputs.FirstOrDefault(x => x.Ref == "oResponse").Value[0][0].Value = Newtonsoft.Json.JsonConvert.SerializeObject(stripeCharge);

                return new ActionableResponse
                {
                    Success = true,
                    ResponseMessages = new System.Collections.Generic.List<ResponseMessage>() { new ResponseMessage() { Message = "Payment is successfull", MessageLevel = MessageLevel.Informational } }
                };
            }
            catch (StripeException ex)
            {
                return new ActionableResponse
                {
                    Success = false,
                    Messages = new System.Collections.Generic.List<string>() { ex.Message },
                    ResponseAction = ResponseAction.Cancel
                };

            }

        }
    }
}