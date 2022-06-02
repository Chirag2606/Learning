namespace Cyara.Web.Portal.Areas.Api.Models.V2_5
{
    using Cyara.Shared.Types.TestCase;

    public partial class TestCase
    {
        public static TestCase From(ChatTestCase testCase)
        {
            if (testCase == null)
            {
                return null;
            }

            return new TestCase
                   {
                       Active = true,
                       Alert = TestCaseAlert.From(testCase),
                       CallFlow = CallFlowType.Inbound,
                       Description = testCase.Description ?? string.Empty,
                       FolderPath = testCase.FolderPath ?? string.Empty,
                       ModifiedBy = V2_2.TestCase.GetModifiedByUsername(testCase.ModifiedBy),
                       Name = testCase.Name,
                       Notes = testCase.Notes ?? string.Empty,
                       Url = testCase.Url ?? string.Empty,
                       TestCaseId = testCase.TestCaseId,
                       Type = Channel.Web
                    };
        }

        public static TestCase From(SmsTestCase testCase)
        {
            if (testCase == null)
            {
                return null;
            }

            return new TestCase
                       {
                           Active = true,
                           Alert = TestCaseAlert.From(testCase),
                           CallFlow = CallFlowType.Inbound,
                           Description = testCase.Description ?? string.Empty,
                           FolderPath = testCase.FolderPath ?? string.Empty,
                           ModifiedBy = V2_2.TestCase.GetModifiedByUsername(testCase.ModifiedBy),
                           Name = testCase.Name,
                           Notes = testCase.Notes ?? string.Empty,
                           Mobile = testCase.Mobile ?? string.Empty,
                           TestCaseId = testCase.TestCaseId,
                           Type = Channel.Sms
                       };
        }

        public static TestCase From(EmailTestCase testCase)
        {
            if (testCase == null)
            {
                return null;
            }

            return new TestCase
                   {
                       Active = true,
                       Alert = TestCaseAlert.From(testCase),
                       CallFlow = CallFlowType.Inbound,
                       Description = testCase.Description ?? string.Empty,
                       FolderPath = testCase.FolderPath ?? string.Empty,
                       ModifiedBy = V2_2.TestCase.GetModifiedByUsername(testCase.ModifiedBy),
                       Name = testCase.Name,
                       Notes = testCase.Notes ?? string.Empty,
                       EmailTo = testCase.EmailTo ?? string.Empty,
                       TestCaseId = testCase.TestCaseId,
                       Type = Channel.Email
            };
        }
    }
}