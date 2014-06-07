using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using S1.Common.Model;
using S1.CommonBiz;

namespace UserManagementUI.Controllers
{
    [HandleError]
    public class HomeController : Controller
    {
        public ActionResult Index(string moduleName)
        {
            // have to use session - think other alternatives if any
            Session[SessionUtility.SessionContainer.USER_ID + "ModuleName"] = moduleName;
            ViewBag.MenuList = @"
                <ul>
                    <li>
                        <a href=' #'>
                            Item 1
                        </a>
                    </li>
                    <li>
                        <a href='#'>Folder 0</a>
                        <ul>
                            <li><a href='#'>Sub Item 1.1</a></li>
                            <li><a href='#'>Sub Item 1.2</a></li>
                            <li><a href='#'>Sub Item 1.3</a></li>
                            <li><a href='#'>Sub Item 1.4</a></li>
                            <li><a href='#'>Sub Item 1.2</a></li>
                            <li><a href='#'>Sub Item 1.3</a></li>
                            <li><a href='#'>Sub Item 1.4</a></li>
                        </ul>
                    </li>
                    <li>
                        <a href='#'>Folder 1</a>
                        <ul>
                            <li><a href='#'>Sub Item 1.1</a></li>
                            <li><a href='#'>Sub Item 1.2</a></li>
                            <li><a href='#'>Sub Item 1.3</a></li>
                            <li><a href='#'>Sub Item 1.4</a></li>
                            <li><a href='#'>Sub Item 1.2</a></li>
                            <li><a href='#'>Sub Item 1.3</a></li>
                            <li><a href='#'>Sub Item 1.4</a></li>
                        </ul>
                    </li>
                    <li><a href='#'>Item 3</a></li>
                    <li>
                        <a href='#'>Folder 2</a>
                        <ul>
                            <li><a href='#'>Sub Item 2.1</a></li>
                            <li>
                                <a href='#'>Folder 2.1</a>
                                <ul>
                                    <li><a href='#'>Sub Item 2.1.1</a></li>
                                    <li><a href='#'>Sub Item 2.1.2</a></li>
                                    <li>
                                        <a href='#'>Folder 3.1.1</a>
                                        <ul>
                                            <li><a href='#'>Sub Item 3.1.1.1</a></li>
                                            <li><a href='#'>Sub Item 3.1.1.2</a></li>
                                            <li><a href='#'>Sub Item 3.1.1.3</a></li>
                                            <li><a href='#'>Sub Item 3.1.1.4</a></li>
                                            <li><a href='#'>Sub Item 3.1.1.5</a></li>
                                        </ul>
                                    </li>
                                    <li><a href='#'>Sub Item 2.1.4</a></li>
                                </ul>
                            </li>
                        </ul>
                    </li>
                    <li><a href='#/style/'>Item 4</a></li>
                </ul>
                <br style='clear: left' />
            ";
            
            return View();
        }

        public ActionResult About()
        {            
            return View();
        }
    }
}