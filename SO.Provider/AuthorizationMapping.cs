using System;
using System.Configuration;
using System.Xml;
using System.Xml.Serialization;
using System.Web;

namespace SOProvider
{
    public class AuthorizationMapping : IConfigurationSectionHandler
    {
        #region Public Properties
        #region Type
        /// <summary>
        /// Gets or sets the type of authorization mapping
        /// </summary>
        /// <value>The type of the assembly to load</value>
        [XmlAttribute("type")]
        public string Type { get; set; }
        #endregion
        #region ConnectionStringKey
        /// <summary>
        /// Gets or sets the connection string key
        /// </summary>
        /// <value>The connection string</value>
        [XmlAttribute("connectionStringKey")]
        public string ConnectionStringKey { get; set; }
        #endregion
        #region NameOfAnonymousRole
        /// <summary>
        /// Gets or sets the nameOfAnonymousRole
        /// </summary>
        /// <value>The connection string</value>
        [XmlAttribute("nameOfAnonymousRole")]
        public string NameOfAnonymousRole { get; set; }
        #endregion
        #endregion        
        #region Constructor
        public AuthorizationMapping()
        {
            
        }
        private AuthorizationMapping(string type, string connectionStringKey, string nameOfAnonymousRole)
        {
            this.Type = type;
            this.ConnectionStringKey = connectionStringKey;
            this.NameOfAnonymousRole = nameOfAnonymousRole;
        }
        #endregion
        #region IConfigurationSectionHandler Members
        #region Create()
        /// <summary>
        /// overridden for create the instance
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="configContext"></param>
        /// <param name="section"></param>        
        /// <returns></returns>
        public object Create(object parent, object configContext, XmlNode section)
        {
            XmlNode nodeType = section.Attributes.RemoveNamedItem("type");
            XmlNode nodeConnectionStringKey = section.Attributes.RemoveNamedItem("connectionStringKey");
            XmlNode nodenameOfAnonymousRole = section.Attributes.RemoveNamedItem("nameOfAnonymousRole");
            return new AuthorizationMapping(nodeType.Value.ToString(), nodeConnectionStringKey.Value.ToString(), nodenameOfAnonymousRole.Value.ToString());
        }
        #endregion
        #endregion
        #region Static Methods
        #region GetAuthorizationMappingSettings()
        /// <summary>
        /// Gets the settings.
        /// </summary>
        /// <returns></returns>
        public static AuthorizationMapping GetAuthorizationMappingSettings()
        {
            return (AuthorizationMapping)ConfigurationManager.GetSection("AuthorizationMapping");           
        }
        #endregion
        #endregion
    }
}