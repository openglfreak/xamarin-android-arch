using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace AndroidSourcesGenerate
{
    public class Program
    {
        public static void Main()
        {
            Assembly xaprepare = Assembly.LoadFile(Path.Combine(Environment.CurrentDirectory, "build-tools", "xaprepare", "xaprepare", "bin", "Release", "xaprepare.exe"));
            Type AndroidToolchain = xaprepare.GetType("Xamarin.Android.Prepare.AndroidToolchain");
            Type BuildInfo = xaprepare.GetType("Xamarin.Android.Prepare.BuildInfo");
            Type Characters = xaprepare.GetType("Xamarin.Android.Prepare.Characters");
            Type Context = xaprepare.GetType("Xamarin.Android.Prepare.Context");
            Type Linux = xaprepare.GetType("Xamarin.Android.Prepare.Linux");
            Type Log = xaprepare.GetType("Xamarin.Android.Prepare.Log");
            Type LoggingVerbosity = xaprepare.GetType("Xamarin.Android.Prepare.LoggingVerbosity");
            Type Paths = xaprepare.GetType("Xamarin.Android.Prepare.Configurables+Paths");
            Type Urls = xaprepare.GetType("Xamarin.Android.Prepare.Configurables+Urls");

            TextWriter consoleOut = Console.Out;
            Console.SetOut(TextWriter.Null);
            Console.SetError(TextWriter.Null);

            Object context = Context.GetProperty("Instance").GetValue(null);

            Context.GetProperty("LoggingVerbosity").SetValue(context, Enum.Parse(LoggingVerbosity, "Silent"));
            Log.GetMethod("SetContext", new Type[] { Context }).Invoke(null, new Object[] { context });

            Context.GetProperty("OS").SetValue(context, Linux.GetMethod("DetectAndCreate").Invoke(null, new Object[] { context }));
            Context.GetField("characters", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(context, Characters.GetMethod("Create").Invoke(null, new Object[] { context }));
            Object buildInfo = BuildInfo.GetConstructor(new Type[0]).Invoke(new Object[0]);
            Context.GetProperty("BuildInfo").SetValue(context, buildInfo);
            ((Task)BuildInfo.GetMethod("GatherGitInfo").Invoke(buildInfo, new Object[] { context })).Wait();

            Uri androidUri = (Uri)AndroidToolchain.GetField("AndroidUri").GetValue(null);
            Object toolchain = AndroidToolchain.GetConstructor(new Type[0]).Invoke(new Object[0]);
            IList components = (IList)AndroidToolchain.GetProperty("Components").GetValue(toolchain);
            foreach (Object component in components)
            {
                String packageName = (String)component.GetType().GetProperty("Name").GetValue(component) + ".zip";
                Uri relativeUrl = (Uri)component.GetType().GetProperty("RelativeUrl").GetValue(component);
                Uri baseUri = (relativeUrl != null) ? new Uri(androidUri, relativeUrl) : androidUri;
                Uri fullUri = new Uri(baseUri, packageName);
                consoleOut.WriteLine(fullUri.AbsoluteUri);
            }

            consoleOut.WriteLine(Urls.GetField("Corretto").GetValue(null));

            {
                Uri monoBaseUri = (Uri)Urls.GetField("MonoArchive_BaseUri").GetValue(null);
                String monoArchiveFileName = (String)Paths.GetProperty("MonoArchiveFileName").GetValue(null);
                String monoArchiveWindowsFileName = (String)Paths.GetProperty("MonoArchiveWindowsFileName").GetValue(null);
                Uri fullUri = new Uri(monoBaseUri, monoArchiveFileName);
                Uri fullWindowsUri = new Uri(monoBaseUri, monoArchiveWindowsFileName);
                consoleOut.WriteLine(fullUri.AbsoluteUri);
                consoleOut.WriteLine(fullWindowsUri.AbsoluteUri);
            }

            // Currently 404's
            /*{
                Uri bundleDownloadPrefix = (Uri)Urls.GetProperty("Bundle_XABundleDownloadPrefix").GetValue(null);
                String bundleFileName = (String)Paths.GetProperty("XABundleFileName").GetValue(null);
                Uri fullUri = new Uri(bundleDownloadPrefix, bundleFileName);
                consoleOut.WriteLine(fullUri.AbsoluteUri);
            }*/
        }
    }
}
