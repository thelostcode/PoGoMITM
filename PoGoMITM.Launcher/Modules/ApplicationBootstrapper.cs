using Nancy;
using Nancy.Conventions;

namespace PoGoMITM.Launcher.Modules
{
    public class ApplicationBootstrapper : DefaultNancyBootstrapper
    {
        protected override void ConfigureConventions(NancyConventions nancyConventions)
        {
            nancyConventions.StaticContentsConventions.Add(StaticContentConventionBuilder.AddDirectory("css", @"css"));
            nancyConventions.StaticContentsConventions.Add(StaticContentConventionBuilder.AddDirectory("js", @"js"));
            base.ConfigureConventions(nancyConventions);
        }
    }
}