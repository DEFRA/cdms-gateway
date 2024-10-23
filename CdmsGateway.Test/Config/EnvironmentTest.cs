using Microsoft.AspNetCore.Builder;

namespace CdmsGateway.Test.Config;

public class EnvironmentTest
{

   [Fact]
   public void IsNotDevModeByDefault()
   {
      var _builder = WebApplication.CreateBuilder();

      var isDev = CdmsGateway.Config.Environment.IsDevMode(_builder);

      Assert.False(isDev);
   }
}
