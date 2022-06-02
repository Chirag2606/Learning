namespace Cyara.Web.Portal.Areas.Api.Models.V2_5
{
    public partial class TestCaseAlert
    {
        public static TestCaseAlert From(Shared.Types.TestCase.ITestCase testCase)
        {
            return AutoMapper.Mapper.Map<V2_2.TestCaseAlert, TestCaseAlert>(V2_2.TestCaseAlert.From(testCase));
        }
    }
}