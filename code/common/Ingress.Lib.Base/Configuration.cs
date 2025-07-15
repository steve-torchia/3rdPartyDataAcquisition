//using System.IO;
//using Microsoft.Extensions.Configuration;

//namespace Ingress.Lib.Base
//{
//    /// <summary>
//    /// Handle parsing configuration files.
//    /// </summary>
//    public static class Configuration
//    {
//        public const string AzureKeyVaultReferencePrefix = "@Microsoft.KeyVault";

//        /// <summary>
//        /// Create an empty configuration
//        /// </summary>
//        public static IConfiguration New()
//        {
//            return new ConfigurationBuilder().Build();
//        }

//        /// <summary>
//        /// Parse a json configuration stream
//        /// </summary>
//        /// <param name="jsonStream">Stream containg json configuration</param>
//        /// <returns></returns>
//        public static IConfiguration ParseJsonConfigurationStream(Stream jsonStream)
//        {
//            return new ConfigurationBuilder().AddJsonStream(jsonStream)
//                                             .Build();
//        }

//        /// <summary>
//        /// Parse a json configuration file
//        /// </summary>
//        public static IConfiguration ParseJsonConfigurationFile(string filePath, bool optional = false)
//        {
//            return new ConfigurationBuilder().AddJsonFile(filePath, optional: optional)
//                                             .Build();
//        }

//        /// <summary>
//        /// Returns a new configuration object with any environment variables added.
//        /// </summary>
//        public static IConfiguration AddEnvironmentVariables(this IConfiguration existingConfig)
//        {
//            return new ConfigurationBuilder().AddConfiguration(existingConfig)
//                                             .AddEnvironmentVariables()
//                                             .Build();
//        }

//        /// <summary>
//        /// Plain wrapper for the `Get` method that lets you convert from configuration to a specific type
//        /// </summary>
//        public static T Bind<T>(this IConfiguration existingConfig)
//        {
//            return existingConfig.Get<T>();
//        }
//    }
//}
