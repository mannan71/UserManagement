using System;
using System.Web.Caching;
using System.Configuration;
using System.IO;
using System.Web;

namespace SOProvider
{
    public class UserInRoleCacheDependency : CacheDependency
    {
        #region Static Member Variables
        private static string _CacheDependencyFilePath = ConfigurationManager.AppSettings["CACHE_DEPENDENCY_FILE_PATH"] + SecurityConstant.UserInRoleCacheDependency + @"\";        
        private FileSystemWatcher _Watcher;        
        #endregion
        #region Constructor
        public UserInRoleCacheDependency()
        {            
            _Watcher = new FileSystemWatcher(_CacheDependencyFilePath);
            _Watcher.EnableRaisingEvents = true;
            _Watcher.Created += new FileSystemEventHandler(OnWatcherCreated);
        }
        #endregion
        #region Private Events
        private void OnWatcherCreated(object sender, FileSystemEventArgs e)
        {
            if (e.Name == SecurityConstant.UserInRoleCacheDependency)
            {
                base.NotifyDependencyChanged(sender, e);
            }
        }
        #endregion
    }
}
