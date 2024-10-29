using Microsoft.AspNetCore.Builder;

namespace CdmsGateway.Test.Config;

public class EnvironmentTest
{
   [Fact]
   public void IsNotDevModeByDefault()
   {
      var builder = WebApplication.CreateBuilder();

      var isDev = CdmsGateway.Config.Environment.IsDevMode(builder);

      Assert.False(isDev);
   }
}
