using Microsoft.Practices.EnterpriseLibrary.Data;
using SOProvider;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Configuration.Provider;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Caching;

namespace SO.Provider
{
 [AspNetHostingPermission(SecurityAction.Demand, Level = AspNetHostingPermissionLevel.Minimal)]
    public class SOSiteMapProvider : SiteMapProvider
    {       
        #region Member Variables
        private SiteMapProvider _ParentSiteMapProvider = null;
        private SiteMapNode _RootNode = null;
        private ArrayList _SiteMapNodes = null;
        private ArrayList _ChildParentRelationship = null;
        private Dictionary<Guid, SiteMapNode> _nodes = new Dictionary<Guid, SiteMapNode>(32);
        private SiteMapNode _CurParent;
        private string _ConnectionString;	    // Database connection string
        private string _ConnectionStringKey;	// Database connection string key
        private string _NameOfAnonymousRole;    // name of role that ere to be consideres anonymoyus access
        #endregion
        #region Constructors
        /// <summary>
        /// Default constructor.
        /// The Name property is initialized in the Initialize method.
        /// </summary>
        public SOSiteMapProvider()
        {

        }
        #endregion
        #region Overridden Properties
        public override SiteMapNode CurrentNode
        {
            get
            {
                string currentUrl = FindCurrentUrl();
                // Find the SiteMapNode that represents the current page.
                SiteMapNode currentNode = FindSiteMapNode(currentUrl);
                return currentNode;
            }
        }
        public override SiteMapNode RootNode
        {
            get
            {
                return _RootNode;
            }
        }
        public override SiteMapProvider ParentProvider
        {
            get
            {
                return _ParentSiteMapProvider;
            }
            set
            {
                _ParentSiteMapProvider = value;
            }
        }
        public override SiteMapProvider RootProvider
        {
            get
            {
                // If the current instance belongs to a provider hierarchy, it
                // cannot be the RootProvider. Rely on the ParentProvider.
                if (this.ParentProvider != null)
                {
                    return ParentProvider.RootProvider;
                }
                // If the current instance does not have a ParentProvider, it is
                // not a child in a hierarchy, and can be the RootProvider.
                else
                {
                    return this;
                }
            }
        }
        #endregion
        #region Overridden Methods
        #region FindSiteMapNode()
        /// <summary>
        /// Implement the FindSiteMapNode method 
        /// </summary>
        /// <param name="rawUrl"></param>
        /// <returns></returns>
        public override SiteMapNode FindSiteMapNode(string rawUrl)
        {
            // Does the root node match the URL?
            if (RootNode.Url == rawUrl)
            {
                return RootNode;
            }
            else
            {
                SiteMapNode candidate = null;
                // Retrieve the SiteMapNode that matches the URL.
                lock (this)
                {
                    candidate = GetNode(_SiteMapNodes, rawUrl);
                }
                return candidate;
            }
        }
        #endregion
        #region GetChildNodes()
        /// <summary>
        /// Implement the GetChildNodes method -- get child nodes of current node 
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public override SiteMapNodeCollection GetChildNodes(SiteMapNode node)
        {
            SiteMapNodeCollection children = new SiteMapNodeCollection();
            // check the current user role, if he have permission to this page (node)
            // then show the page					
            if (!DoSecurityTrimming(node))
                return children;

            // Iterate through the ArrayList and find all nodes that have the specified node as a parent.
            lock (this)
            {
                for (int i = 0; i < _ChildParentRelationship.Count; i++)
                {
                    string childId = ((DictionaryEntry)_ChildParentRelationship[i]).Key.ToString();
                    SiteMapNode parent = ((DictionaryEntry)_ChildParentRelationship[i]).Value as SiteMapNode;
                    if (parent.Key == node.Key)
                    {
                        SiteMapNode child = GetNode(childId);
                        // check the current user role, if he have permission to this page (node)
                        // then show the page
                        if (DoSecurityTrimming(child))
                            children.Add(child as SiteMapNode);
                    }
                }
            }
            return children;
        }
        #endregion
        #region GetRootNodeCore()
        /// <summary>
        /// Get the Root Node
        /// </summary>
        /// <returns></returns>
        protected override SiteMapNode GetRootNodeCore()
        {
            return RootNode;
        }
        #endregion
        #region GetParentNode()
        /// <summary>
        /// Implement the GetParentNode method -- return parent node of current node 
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public override SiteMapNode GetParentNode(SiteMapNode node)
        {
            // Check the childParentRelationship table and find the parent of the current node.
            // If there is no parent, the current node is the RootNode.
            SiteMapNode parent = null;
            lock (this)
            {
                // Get the Value of the node in childParentRelationship
                parent = GetNode(_ChildParentRelationship, node.Url);
            }
            return parent;
        }
        #endregion
        #endregion
        #region Initialize()
        /// <summary>
        /// Implement the ProviderBase.Initialize property.
        /// Initialize is used to initialize the state that the Provider holds, but
        /// not actually build the site map.  
        /// </summary>
        /// <param name="name"></param>
        /// <param name="attributes"></param>
        public override void Initialize(string name, NameValueCollection config)
        {
            lock (this)
            {
                // Verify that config isn't null
                if (config == null)
                    throw new ArgumentNullException("web.config setting is invalid for SoftOne.Provider.SiteMapProvider");

                // Assign the provider a default name if it doesn't have one
                if (String.IsNullOrEmpty(name))
                    name = "SiteMapProvider";

                // Add a default "description" attribute to config if the
                // attribute doesn’t exist or is empty
                if (string.IsNullOrEmpty(config["description"]))
                {
                    config.Add("description", "SiteMapProvider");
                    config.Remove("description");
                }

                // anonymous role
                if (string.IsNullOrEmpty(config["nameOfAnonymousRole"]))
                {
                    throw new ProviderException("nameOfAnonymousRole cannot be blank at SoftOne.Provider.RoleProvider");

                }
                else
                {
                    _NameOfAnonymousRole = config["nameOfAnonymousRole"].ToString();
                    config.Remove("nameOfAnonymousRole");
                }

                // Initialize _connect                 
                ConnectionStringSettings objConnectionStringSettings = ConfigurationManager.ConnectionStrings[config["connectionStringName"]];
                if (objConnectionStringSettings == null || string.IsNullOrEmpty(objConnectionStringSettings.ConnectionString.Trim()))
                {
                    throw new ProviderException("connectionStringName cannot be blank at SoftOne.Provider.RoleProvider");
                }
                else
                {
                    _ConnectionString = objConnectionStringSettings.ConnectionString;
                    _ConnectionStringKey = config["connectionStringName"].ToString();
                    config.Remove("connectionStringName");

                    // SiteMapProvider processes the securityTrimmingEnabled
                    // attribute but fails to remove it. Remove it now so we can
                    // check for unrecognized configuration attributes.
                    if (config["securityTrimmingEnabled"] != null)
                        config.Remove("securityTrimmingEnabled");
                    // Throw an exception if unrecognized attributes remain
                    if (config.Count > 0)
                    {
                        string attr = config.GetKey(0);
                        if (!String.IsNullOrEmpty(attr))
                            throw new ProviderException("Unrecognized attribute: " + attr);
                    }

                    // create the ArrayList
                    _SiteMapNodes = new ArrayList();
                    _ChildParentRelationship = new ArrayList();
                    // Build the site map in memory
                    LoadSiteMapFromStore();

                    // call base to initialize
                    base.Initialize(name, config);
                }
            }
        }
        #endregion
        #region Protected Methods : LoadSiteMapFromStore()
        /// <summary>
        /// Load SiteMap data from Sql Server
        /// </summary>
        protected virtual void LoadSiteMapFromStore()
        {
            lock (this)
            {
                // If a root node exists, LoadSiteMapFromStore has already
                // been called, and the method can return.
                if (_RootNode != null)
                {
                    return;
                }
                else
                {
                    // Query the database for site map nodes
                    try
                    {
                        string sSql = @"SELECT MenuId,MenuName,FastPath,ParentMenuId,ActionName,ActionPath,SortOrder,IsModule,MenuText
                                        FROM
                                        (
                                        SELECT MenuId,MenuName,FastPath,ParentMenuId,ActionName,ActionPath,SortOrder,IsModule,MenuText
                                        FROM Menu
                                        LEFT OUTER JOIN
                                        Action
                                        ON Menu.ActionId = Action.ActionId
                                        WHERE ParentMenuId IS NULL
                                        UNION
                                        SELECT MenuId,MenuName,FastPath,ParentMenuId,ActionName,ActionPath,SortOrder,IsModule,MenuText
                                        FROM Menu
                                        LEFT OUTER JOIN
                                        Action
                                        ON Menu.ActionId = Action.ActionId
                                        WHERE ParentMenuId IS NOT NULL
                                        AND IsSecurityMenu = @IsSecurityMenu
                                        AND Menu.IsDeleted = 0
                                        )
                                        AS T
                                        ORDER BY IsModule Desc, ParentMenuId, SortOrder";
                        DataTable dt = new DataTable();
                        Database db = DatabaseFactory.CreateDatabase(_ConnectionStringKey);
                        DbCommand cmd = db.GetSqlStringCommand(sSql);
                        db.AddInParameter(cmd, "IsSecurityMenu", DbType.Decimal, Convert.ToBoolean(ConfigurationSettings.AppSettings["SECURITYMODULE"].ToString()));
                        using (DbConnection cn = db.CreateConnection())
                        {
                            cn.Open();
                            try
                            {
                                using (IDataReader dr = db.ExecuteReader(cmd))
                                {
                                    // load DataTable
                                    dt.Load(dr);
                                    // load the MenuInRole. Because cache has expired.
                                    // MenuIsInRole depends on SiteMap for cache expiration.
                                    MenuInRole.GetAllMenuIsInRole(dt);
                                }
                            }
                            catch
                            {
                                throw;
                            }
                            finally
                            {
                                cn.Close();
                            }
                        }

                        // Clear the state of the collections and rootNode
                        _RootNode = null;
                        _SiteMapNodes.Clear();
                        _ChildParentRelationship.Clear();
                        SiteMapNode temp = null;

                        foreach (DataRow dr in dt.Rows)
                        {
                            ArrayList al = new ArrayList(dr.ItemArray);
                            temp = CreateSiteMapNode(dr, dt);
                            // Is this a root node yet?
                            // there will be only one root
                            if (_RootNode == null)
                            {
                                _RootNode = temp;
                                _CurParent = _RootNode;
                                // add root node
                                _SiteMapNodes.Add(new DictionaryEntry(temp.Key, temp));
                            }
                            // If not the root node, add the node to the various collections.
                            else
                            {
                                // add to node collection
                                _SiteMapNodes.Add(new DictionaryEntry(temp.Key, temp));
                                // find parent of current node
                                _CurParent = FindParent(dr["ParentMenuId"].ToString(), dt);
                                // add to root node - we may add a node as parent to the
                                // _ChildParentRelationship before adding it to _SiteMapNodes
                                // because the DB is not returning data in orderly manner
                                _ChildParentRelationship.Add(new DictionaryEntry(temp.Key, _CurParent));
                            }
                        }

                        // cache data                        
                        HttpRuntime.Cache.Insert(SecurityConstant.SiteMapCacheDependency, new object(), new SiteMapCacheDependency(),
                                Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration, CacheItemPriority.NotRemovable,
                                new CacheItemRemovedCallback(OnSiteMapChanged));
                    }
                    finally
                    {
                        //
                    }
                }
            }
            return;
        }
        #endregion
        #region Protected Methods : OnSiteMapChanged()
        /// <summary>
        /// If the Cache is invalidated then reload the site map         
        /// </summary>
        /// <param name="key"></param>
        /// <param name="item"></param>
        /// <param name="reason"></param>
        void OnSiteMapChanged(string key, object item, CacheItemRemovedReason reason)
        {
            lock (this)
            {
                if (key.IndexOf(SecurityConstant.SiteMapCacheDependency) != -1 && reason == CacheItemRemovedReason.DependencyChanged)
                {
                    // Refresh the site map
                    _SiteMapNodes.Clear();
                    _ChildParentRelationship.Clear();
                    _nodes.Clear();
                    _RootNode = null;
                    // this does not called the initialize method automatically
                    // so explicitly reload the SiteMap
                    LoadSiteMapFromStore();
                }
            }
        }
        #endregion
        #region Private Helper Methods
        #region CreateSiteMapNode()
        /// <summary>
        /// create site map from Data Reader
        /// the node contains lso the role info
        /// </summary>        
        /// <returns></returns>
        private SiteMapNode CreateSiteMapNode(DataRow dr, DataTable dt)
        {
            // MenuId -> key
            if (dr["MenuId"] == null || dr["MenuId"].ToString().Trim() == string.Empty)
                throw new ProviderException("Missing Node Id at SoftOne.Provider.SiteMapProvider");
            Guid menuId = new Guid(dr["MenuId"].ToString());
            if (_nodes.ContainsKey(menuId)) // Make sure the node ID is unique
                throw new ProviderException("Duplicate Node Id at SoftOne.Provider.SiteMapProvider");

            // ActionPath -> url
            string actionPath = null;
            if (dr["ActionPath"] == null || dr["ActionPath"].ToString().Trim() == string.Empty)
            {
                actionPath = null;
            }
            else
            {
                string[] tempPath = dr["ActionPath"].ToString().Trim().Split('/');
                actionPath = "~/" + tempPath[0] + SecurityConstant.RewriteURL + "/" + tempPath[1];
                
                // actionPath = "~/" + dr["ActionPath"].ToString().Trim() + SecurityConstant.RewriteURL;
                // add the MenuId to ationPath to enable expand collapse 
                // state preservation after post back or redirection
                //actionPath += "?SiteMapTreeExpanded=" + menuId;//Commented by Anwar: Dated 25/11/09 on account of parameter pass with ActionPath
            }

            // title -> MenuText or MenuName
            string title = dr.IsNull("MenuText") ? dr["MenuName"].ToString() : dr["MenuText"].ToString();
            
            // ActionName -> Description
            string actionName = string.Empty;
            if (dr["ActionName"] == null || dr["ActionName"].ToString().Trim() == string.Empty)
            {
                actionName = string.Empty;
            }
            else
            {
                actionName = dr["ActionName"].ToString();
            }
            // roleList -> Roles
            string[] roleList = null;
            roleList = MenuInRole.MenuIsInRole(dt, menuId);
            // replace the Anonymous role with * sign to enable sitemap to work
            for (int i = 0; i < roleList.Length; i++)
            {
                if (roleList[i].Equals(_NameOfAnonymousRole))
                    roleList[i] = "*";
            }
            // create the node
            SiteMapNode node = new SiteMapNode(this, menuId.ToString(), actionPath, title, actionName, roleList, null, null, null);

            return node;
        }
        #endregion
        #region FindParent()
        /// <summary>
        /// Get a parent SiteMapNode for current node.
        /// This is risky bcz we may find a node that is not yet add to the _SiteMapNodes
        /// </summary>
        /// <param name="parentMenuId">parent menu id</param>
        /// <param name="dt">DataTable containing the entire SiteMap data from DB</param>
        /// <returns></returns>
        private SiteMapNode FindParent(string parentMenuId, DataTable dt)
        {
            // get the specific parent node (always 1 so selet the 0th)
            DataRow dr = dt.Select("MenuId='" + parentMenuId + "'")[0];
            return CreateSiteMapNode(dr, dt);
        }
        #endregion
        #region GetNode()
        /// <summary>
        /// Get a node from List using node url or key
        /// the url can be a valid url or it can be node id (Guid) from Dictionary
        /// </summary>
        /// <param name="list"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        private SiteMapNode GetNode(ArrayList list, string url)
        {
            if (url.IndexOf("~") != -1)	// a url
            {
                for (int i = 0; i < list.Count; i++)
                {
                    DictionaryEntry item = (DictionaryEntry)list[i];
                    if (((SiteMapNode)item.Value).Url == url)
                        return item.Value as SiteMapNode;
                }
            }
            else	// node Id (Guid) from dictionary
            {
                for (int i = 0; i < list.Count; i++)
                {
                    DictionaryEntry item = (DictionaryEntry)list[i];
                    if (((SiteMapNode)item.Value).Key == url)
                        return item.Value as SiteMapNode;
                }
            }
            return null;
        }
        /// <summary>
        /// overloaded, get a node using node Id 
        /// </summary>
        /// <param name="nodeId"></param>
        /// <returns>SiteMapNode</returns>
        private SiteMapNode GetNode(string nodeId)
        {
            for (int i = 0; i < _SiteMapNodes.Count; i++)
            {
                DictionaryEntry item = (DictionaryEntry)_SiteMapNodes[i];
                if (item.Key.ToString() == nodeId)
                    return item.Value as SiteMapNode;
            }
            return null;
        }
        #endregion
        #region FindCurrentUrl()
        /// <summary>
        /// Get the URL of the currently displayed page.
        /// </summary>
        /// <returns></returns>			
        private string FindCurrentUrl()
        {
            try
            {
                // The current HttpContext.
                HttpContext currentContext = HttpContext.Current;
                if (currentContext != null)
                {
                    // get the app length					
                    int appLen = HttpContext.Current.ApplicationInstance.Request.ApplicationPath.Length;
                    return "~" + currentContext.Request.Url.AbsolutePath.Substring(appLen);
                }
                else
                {
                    throw new ProviderException("HttpContext.Current is Invalid for SoftOne.Provider.SiteMapProvider");
                }
            }
            catch (Exception e)
            {
                throw new NotSupportedException("The provider SoftOne.Provider.SiteMapProvider requires a valid context.", e);
            }
        }
        #endregion
        #region DoSecurityTrimming()
        /// <summary>
        /// check the current user role, if he have permission to this page (node)
        /// then show the page. Moreover it also add the page & Page Index to _dtWebPageCodePath
        /// Data Table.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private bool DoSecurityTrimming(SiteMapNode node)
        {
            if (node.Roles != null)
            {
                foreach (string role in node.Roles)
                {
                    if (role == "*")	// if anonymous access is allowed
                        return true;
                    // if user has role
                    if (SOUserInRole.UserIsInRole(role))
                        return true;
                }
            }
            return false;
        }
        #endregion
        #endregion
    }
}
