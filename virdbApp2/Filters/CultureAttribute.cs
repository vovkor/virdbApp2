using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Globalization;
using System.Threading;
using System.Web.Mvc;


namespace virdbApp2.Filters
{
    public class CultureAttribute : FilterAttribute, IActionFilter
    {
        //Метод OnActionExecuted срабатывает после вызова действия контроллера. В начале он получает установленную культуру из куков 
        // https://msdn.microsoft.com/ru-ru/library/dd381609(v=vs.100).aspx
        // https://metanit.com/sharp/mvc/16.2.php
        public void OnActionExecuting(ActionExecutingContext filterContext)
        {
            string cultureName = null;
            // Получаем куки из контекста, которые могут содержать установленную культуру
            HttpCookie cultureCookie = filterContext.HttpContext.Request.Cookies["lang"];
            if (cultureCookie != null)
                cultureName = cultureCookie.Value;
            else
                cultureName = "ru";

            // Список культур
            List<string> cultures = new List<string>() { "ru", "en", "de" };
            if (!cultures.Contains(cultureName))
            {
                cultureName = "ru";
            }
            // Учстновка языка
            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(cultureName);
            Thread.CurrentThread.CurrentUICulture = CultureInfo.CreateSpecificCulture(cultureName);
        }

        public void OnActionExecuted(ActionExecutedContext filterContext)
        {
            //не реализован
        }
    }

}