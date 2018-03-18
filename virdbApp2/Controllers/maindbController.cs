using PagedList; // vovkor
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using virdbApp2.Models;
using Microsoft.AspNet.Identity;
// vovkor для RequiredLabel, чтобы обязательные роля были со звездочками  НАЧАЛО
using System.Linq.Expressions;
using System.ComponentModel;
using virdbApp2.Filters;

public static class RequiredLabel
{
    public static MvcHtmlString RequiredLabelFor<TModel, TValue>(this HtmlHelper<TModel> helper, Expression<Func<TModel, TValue>> expression, object htmlAttributes)
    {
        var metaData = ModelMetadata.FromLambdaExpression(expression, helper.ViewData);

        string htmlFieldName = ExpressionHelper.GetExpressionText(expression);
        string labelText = metaData.DisplayName ?? metaData.PropertyName ?? htmlFieldName.Split('.').Last();

        if (metaData.IsRequired)
            labelText += "<span style='color:red' class=\"required\">&nbsp;*</span>";

        if (String.IsNullOrEmpty(labelText))
            return MvcHtmlString.Empty;

        var label = new TagBuilder("label");
        label.Attributes.Add("for", helper.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldId(htmlFieldName));

        foreach (PropertyDescriptor prop in TypeDescriptor.GetProperties(htmlAttributes))
        {
            label.MergeAttribute(prop.Name.Replace('_', '-'), prop.GetValue(htmlAttributes).ToString(), true);
        }

        label.InnerHtml = labelText;
        return MvcHtmlString.Create(label.ToString());
    }

}
// vovkor для RequiredLabel, чтобы обязательные роля были со звездочками  КОНЕЦ


// перенести в отдельный файл? Правой кнопкой на проекте -> Добавить -> Класс
// Dynamic LINQ where clause Динамический запрос
public class FilterBuilder
{
    public FilterBuilder()
    { }
    public Expression Build(Expression member, string propertyName, string searchStr1, string searchStrOr1, string searchStrOr11)
    {
        var miTL = typeof(String).GetMethod("ToLower", Type.EmptyTypes);
        Expression condition = null;

        foreach (var part in propertyName.Split('.'))
        {
            member = Expression.PropertyOrField(member, part);
        }
        member = Expression.Call(member, miTL);
        condition = Expression.Call(member, typeof(string).GetMethod("Contains"), Expression.Constant(searchStr1.ToLower())); //searchStr1

        if (String.IsNullOrEmpty(searchStrOr1) == false)
        {
            condition = Expression.Or(condition,
                              Expression.Call(member,
                                              typeof(string).GetMethod("Contains"),
                                              Expression.Constant(searchStrOr1.ToLower())));
        }

        if (String.IsNullOrEmpty(searchStrOr11) == false)
        {
            condition = Expression.Or(condition,
                              Expression.Call(member,
                                              typeof(string).GetMethod("Contains"),
                                              Expression.Constant(searchStrOr11.ToLower())));
        }
        return condition;
    }

    public Expression Build1(Expression member, string propertyName, string searchStr1)
    {
        var miTL = typeof(String).GetMethod("ToLower", Type.EmptyTypes);
        Expression condition = null;

        foreach (var part in propertyName.Split('.'))
        {
            member = Expression.PropertyOrField(member, part);
        }
        member = Expression.Call(member, miTL);
        condition = Expression.Call(member, typeof(string).GetMethod("Contains"), Expression.Constant(searchStr1.ToLower())); //searchStr1
        return condition;
    }
}


namespace virdbApp2.Controllers
{
    [Culture]
    public class maindbController : Controller
    {
        private virdb2Entities db = new virdb2Entities();

        // GET: /maindb/
        // правлю Index
        //public ActionResult Index()
        // Для поиска
        static List<SearchField> SearchFields = new List<SearchField>();


        public ActionResult Index(string sortOrder, string currentFilter, string searchString, int? page, int? genusChoice, string txtMaxRec,
            string searchField1,
            string searchStr1,
            string searchStrOr1,
            string searchStrOr11,
            string searchField2,
            string searchStr2,
            string searchStrOr2,
            string searchStrOr22,
            string searchField3,
            string searchStr3,
            string searchStrOr3,
            string searchStrOr33,
            string searchField4,
            string searchStr4,
            string searchStrOr4,
            string searchStrOr44,
            string searchField5,
            string searchStr5,
            string searchStrOr5,
            string searchStrOr55,
            string enable_advanced_search

           ) // vovkor добавил аргументы
        {

            int max_records;
            if (SearchFields.Count == 0) { 
                SearchFields.Add(new SearchField()
                {
                    code = "accenumb",
                    name = "Номер образца",
                    exp = "accenumb"
                });
                SearchFields.Add(new SearchField()
                {
                    code = "nameRus",
                    name = "Название",
                    exp = "accename_rus"
                });
                SearchFields.Add(new SearchField()
                {
                    code = "genus",
                    name = "Род",
                    exp = "botanic.taxonomy_genus.genus_name" // ??? проваливаемся через имена таблиц
                });
                SearchFields.Add(new SearchField()
                {
                    code = "spesies",
                    name = "Вид",
                    exp = "botanic.species"
                });
                SearchFields.Add(new SearchField()
                {
                    code = "country",
                    name = "Страна происхождения",
                    exp = "geography.GEORUS"
                });
            }
            ViewBag.searchField1 = new SelectList(SearchFields, "code", "name", "accenumb"); // со значением по умолчанию
            ViewBag.searchField2 = new SelectList(SearchFields, "code", "name", "nameRus");
            ViewBag.searchField3 = new SelectList(SearchFields, "code", "name", "genus");
            ViewBag.searchField4 = new SelectList(SearchFields, "code", "name", "spesies");
            ViewBag.searchField5 = new SelectList(SearchFields, "code", "name", "country");

            if (TempData["strFromEdit"] != null) // пришли из Edit
            {
                // чтобы поля поиска не сбрасывались
                if (TempData["strSearch"] != null) { searchString = TempData["strSearch"].ToString(); }
                if (TempData["searchField1"] != null) { searchField1 = TempData["searchField1"].ToString(); }
                if (TempData["searchStr1"] != null) { searchStr1 = TempData["searchStr1"].ToString(); }
                if (TempData["searchStrOr1"] != null) { searchStrOr1 = TempData["searchStrOr1"].ToString(); }
                if (TempData["searchStrOr11"] != null) { searchStrOr11 = TempData["searchStrOr11"].ToString(); }
                if (TempData["searchField2"] != null) { searchField2 = TempData["searchField2"].ToString(); }
                if (TempData["searchStr2"] != null) { searchStr2 = TempData["searchStr2"].ToString(); }
                if (TempData["searchStrOr2"] != null) { searchStrOr2 = TempData["searchStrOr2"].ToString(); }
                if (TempData["searchStrOr22"] != null) { searchStrOr22 = TempData["searchStrOr22"].ToString(); }
                if (TempData["searchField3"] != null) { searchField3 = TempData["searchField3"].ToString(); }
                if (TempData["searchStr3"] != null) { searchStr3 = TempData["searchStr3"].ToString(); }
                if (TempData["searchStrOr3"] != null) { searchStrOr3 = TempData["searchStrOr3"].ToString(); }
                if (TempData["searchStrOr33"] != null) { searchStrOr33 = TempData["searchStrOr33"].ToString(); }
                if (TempData["searchField4"] != null) { searchField4 = TempData["searchField4"].ToString(); }
                if (TempData["searchStr4"] != null) { searchStr4 = TempData["searchStr4"].ToString(); }
                if (TempData["searchStrOr4"] != null) { searchStrOr4 = TempData["searchStrOr4"].ToString(); }
                if (TempData["searchStrOr44"] != null) { searchStrOr44 = TempData["searchStrOr44"].ToString(); }
                if (TempData["searchField5"] != null) { searchField5 = TempData["searchField5"].ToString(); }
                if (TempData["searchStr5"] != null) { searchStr5 = TempData["searchStr5"].ToString(); }
                if (TempData["searchStrOr5"] != null) { searchStrOr5 = TempData["searchStrOr5"].ToString(); }
                if (TempData["searchStrOr55"] != null) { searchStrOr55 = TempData["searchStrOr55"].ToString(); }
            }


            // добавил сортировку
            ViewBag.CurrentSort = sortOrder;
            // Зачем String.IsNullOrEmpty()  ???
            //ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "name_asc";
            ViewBag.NameSortParm = sortOrder == "name_asc" ? "name_desc" : "name_asc";
            ViewBag.NameRusSortParm = sortOrder == "name_rus_asc" ? "name_rus_desc" : "name_rus_asc";
            ViewBag.AccenumbSortParm = sortOrder == "accenumb_asc" ? "accenumb_desc" : "accenumb_asc";
            ViewBag.DateSortParm = sortOrder == "Date" ? "date_desc" : "Date";
            ViewBag.CoordSortParm = sortOrder == "coord_asc" ? "coord_desc" : "coord_asc";
            ViewBag.CropSortParm = sortOrder == "crop_asc" ? "crop_desc" : "crop_asc";

            // Работа с Родами
            if (TempData["strGenus"] != null && TempData["strFromEdit"] != null) // пришли из Edit
            {
                // Если пришли из другой формы, то genusChoice и page = null. Возьмем их из TempData
                // Объект TempData похож на данные Session за исключением того, что значения TempData помечаются для удаления, когда они прочитаны
                // Если вы устанавливаете TempData в методе Index, но в представлении Index не выводите, а выводите, к примеру, в представлении Contact, то вы можете там вывести также сколько угодно раз, но в других представлениях также уже нельзя будет получить это значение, так как оно уже получено.
                genusChoice = Convert.ToInt32(TempData["strGenus"].ToString()); // 
                ViewBag.genusChoice = new SelectList(db.taxonomy_genus.OrderBy(x => x.genus_name), "id", "genus_name", TempData["strGenus"]);
            }
            else
            {
                if (genusChoice == null)
                {
                    // Если null, то выбираем значение по умолчанию с id==1. Если надо выбрать все записи, в таблице есть запись с id == 0 и N3 == пусто
                    ViewBag.genusChoice = new SelectList(db.taxonomy_genus.OrderBy(x => x.genus_name), "id", "genus_name", 1); // значение по умолчанию 1 db.GerbRods.Where(g => g.id.Equals(1)).First().id
                    genusChoice = 1;
                }
                else
                {
                    ViewBag.genusChoice = new SelectList(db.taxonomy_genus.OrderBy(x => x.genus_name), "id", "genus_name", genusChoice);
                }

            }

            // Получается, что genusChoice и ViewBag.genusChoice - это разные переменные
            // В genusChoice приходит выбранный в выпадающем списке ID genus. 
            // Пользователь выбирает в @Html.DropDownList("genusChoice"), это значение принимает контроллер, 
            // потому что Index имеет аргумент genusChoice. Дальше genusChoice использую в запросе .Where(g => g.N1______ == genusChoice
            // Чтобы заполнить выпадающий список, присваиваю ViewBag.genusChoice =  new SelectList()

            // При нажатии на номер страницы во view (@Html.PagedListPager), надо передать текущий genus, сделаю это через ViewBag.currentGenus

            ViewBag.currentGenus = genusChoice;
            TempData["strGenus"] = genusChoice;

           
            if (TempData["page"] != null && TempData["strFromEdit"] != null) // пришли из Edit
            {
                page = Convert.ToInt32(TempData["page"].ToString());
            }
            TempData["page"] = page;


            // для PagedList
            if (searchString != null)
            {
                //page = 1;
             }
            else
            {
                searchString = currentFilter;
            }

            // чтобы поля поиска не сбрасывались
            ViewBag.CurrentFilter = searchString;  //Зачем? и так сохраняется в поле
            ViewBag.enable_advanced_search = enable_advanced_search;

            if (searchStr1 != null)
            {
                ViewBag.searchField1 = new SelectList(SearchFields, "code", "name", SearchFields.Where(g => g.code.Contains(searchField1)).First().code); // со значением по умолчанию
                ViewBag.searchStr1 = searchStr1; //Зачем? и так сохраняется в поле
                ViewBag.searchStrOr1 = searchStrOr1; //Зачем? и так сохраняется в поле
                ViewBag.searchStrOr11 = searchStrOr11; //Зачем? и так сохраняется в поле
            }
            if (searchStr2 != null)
            {
                ViewBag.searchField2 = new SelectList(SearchFields, "code", "name", SearchFields.Where(g => g.code.Equals(searchField2)).First().code);
                ViewBag.searchStr2 = searchStr2;
                ViewBag.searchStrOr2 = searchStrOr2;
                ViewBag.searchStrOr22 = searchStrOr22;
            }
            if (searchStr3 != null)
            {
                ViewBag.searchField3 = new SelectList(SearchFields, "code", "name", SearchFields.Where(g => g.code.Equals(searchField3)).First().code);
                ViewBag.searchStr3 = searchStr3;
                ViewBag.searchStrOr3 = searchStrOr3;
                ViewBag.searchStrOr33 = searchStrOr33;
            }
            if (searchStr4 != null)
            {
                ViewBag.searchField4 = new SelectList(SearchFields, "code", "name", SearchFields.Where(g => g.code.Equals(searchField4)).First().code);
                ViewBag.searchStr4 = searchStr4;
                ViewBag.searchStrOr4 = searchStrOr4;
                ViewBag.searchStrOr44 = searchStrOr44;
            }
            if (searchStr5 != null)
            {
                ViewBag.searchField5 = new SelectList(SearchFields, "code", "name", SearchFields.Where(g => g.code.Equals(searchField5)).First().code);
                ViewBag.searchStr5 = searchStr5;
                ViewBag.searchStrOr5 = searchStrOr5;
                ViewBag.searchStrOr55 = searchStrOr55;
            }

            
            // считаю количество записей в запросе vovkor 21.02.2015
            //ViewBag.countRecords = db.maindb.Include(m => m.botanic).Include(m => m.collsrc1).Include(m => m.geography).Include(m => m.geography1).Include(m => m.INSTITUT).Include(m => m.INSTITUT1).Include(m => m.INSTITUT2).Include(m => m.KRIS_EXP).Include(m => m.liffom1).Include(m => m.sampstat_full).Include(m => m.storage1).Count();
            
            // выбираем первые 200 записей ?
            var maindb = db.maindb.Include(m => m.botanic).Include(m => m.collsrc1).Include(m => m.geography).Include(m => m.geography1).Include(m => m.INSTITUT).Include(m => m.INSTITUT1).Include(m => m.INSTITUT2).Include(m => m.KRIS_EXP).Include(m => m.liffom1).Include(m => m.sampstat_full).Include(m => m.storage1)
                .Where(m => m.botanic.taxonomy_genus.id == genusChoice || genusChoice == null || genusChoice == 1); // genusChoice //.Take(200);
            
            //  VAV 20.02.2015:
            var t_usr = User.Identity.GetUserName();
            if (t_usr != "") // может использовать HasValue или String.IsNullOrEmpty ?  == надо заменить на Equals    if(str != null && srt.length()!=0)
            {
                // считаю количество записей в запросе vovkor 21.02.2015    
                ViewBag.countRecords = maindb.Where(m => m.owner_str.Equals(t_usr)).Count();
                maindb = maindb.Where(m => m.owner_str.Equals(t_usr));//.Take(200);
            }
            
          
            // поиск
            var search1 = "";

            if (!String.IsNullOrEmpty(searchString))
            {
                TempData["strSearch"] = searchString; // запомню поисковую строку

                    string[] split2 = searchString.Split(new Char[] { ' ' });
                    foreach (string s in split2)
                    {
                        if (s.Trim() != "")
                        {
                            search1 = s.ToUpper().Trim();
                            // gerbs это IQueryable, все where суммируются, запрос оптимизируется, и только потом происходит выборка из базы данных, в отличие от IEnumerable
                            // IQueryable отправляет только отфильтрованные данные клиенту
                            // интерфейс IQueryable наследуется от IEnumerable
                            // В лямбда-выражениях используется лямбда-оператор =>, который читается как «переходит в»
                            // каждый WHERE добавляет результат к предыдущему 
                            maindb = maindb.Where(
                                                    m => (m.accenumb.ToUpper().Contains(search1)
                                                        || m.accename_eng.ToUpper().Contains(search1)
                                                        || m.accename_rus.ToUpper().Contains(search1)
                                                        || m.acqdate.ToString().Contains(search1)
                                                        || m.botanic.taxonomy_crop.crop_name_eng.ToUpper().Contains(search1)
                                                        || m.botanic.taxonomy_crop.crop_name_rus.ToUpper().Contains(search1)
                                                        || m.botanic.taxonomy_genus.taxonomy_family.family_name.ToUpper().Contains(search1)
                                                        || m.botanic.taxonomy_genus.genus_name.ToUpper().Contains(search1)
                                                        || m.botanic.species.ToUpper().Contains(search1)
                                                        || m.botanic.species.ToUpper().Contains(search1)
                                                        || m.collsite_eng.ToUpper().Contains(search1)
                                                        || m.collsite_rus.ToUpper().Contains(search1)
                                                        || m.geography.GEO_ENG.ToUpper().Contains(search1)
                                                        || m.geography.GEORUS.ToUpper().Contains(search1)
                                                        )
                                                        //  VAV 21.02.2015 - если авторизован, то фильтруем по пользователю:
                                                    && (((t_usr != "") && m.owner_str.Equals(t_usr))
                                                        || (t_usr == "")
                                                        )

                                                   );
                        }
                    }
               
            }




            //Если разработчику нужен весь набор возвращаемых данных, то лучше использовать IEnumerable, предоставляющий максимальную скорость. 
            //Если же нам не нужен весь набор, а то только некоторые отфильтрованные данные, то лучше применять IQueryable. 

            // выпадающие списки

            // Build Where Clause Dynamically in Linq
            //searchStr1 = "1"; // для тестов
            //searchStr2 = "2"; // для тестов

            var miTL = typeof(String).GetMethod("ToLower", Type.EmptyTypes); // можно убрать
            var param = Expression.Parameter(typeof(maindb), "m");
            Expression member = param;

            Expression condition = null;
            if (String.IsNullOrEmpty(searchStr1) == false)
            {
                var propertyName = SearchFields.Where(g => g.code.Contains(searchField1.Trim())).First().exp.Trim(); // "GerbRod.N3"

                FilterBuilder fb = new FilterBuilder();
                condition = fb.Build(member, propertyName, searchStr1, searchStrOr1, searchStrOr11);
                var l = Expression.Lambda<Func<maindb, bool>>(condition, param);  //m => m.GerbRod.N3.ToLower().Contains("1") OR  m.GerbRod.N3.ToLower().Contains("2")
                //Expression<Func<gerb, bool>> 
                //l.Compile(); // вернёт тип Func<> 

                // Если where принимает аргумент типа Func<>, возвращает тип IEnumerable, а gerbs у нас IQueyable поэтому надо передавать Expression Func<>
                maindb = maindb.Where(l); // если нужен тип IEnumerable, то gerbs.Where(l.Compile());
                //gerbs = gerbs.Where(l.Compile()); // так не компилируется 

                TempData["searchField1"] = searchField1;
                TempData["searchStr1"] = searchStr1; // запомню поисковую строку
                TempData["searchStrOr1"] = searchStrOr1; // запомню поисковую строку
                TempData["searchStrOr11"] = searchStrOr11; // запомню поисковую строку

            }

            condition = null;
            if (String.IsNullOrEmpty(searchStr2) == false)
            {
                var propertyName = SearchFields.Where(g => g.code.Contains(searchField2.Trim())).First().exp.Trim(); // "GerbRod.N3"

                FilterBuilder fb = new FilterBuilder();
                condition = fb.Build(member, propertyName, searchStr2, searchStrOr2, searchStrOr22);
                var l = Expression.Lambda<Func<maindb, bool>>(condition, param);
                maindb = maindb.Where(l); // если нужен тип IEnumerable, то gerbs.Where(l.Compile());

                TempData["searchField2"] = searchField2;
                TempData["searchStr2"] = searchStr2; // запомню поисковую строку
                TempData["searchStrOr2"] = searchStrOr2; // запомню поисковую строку
                TempData["searchStrOr22"] = searchStrOr22; // запомню поисковую строку
            }

            condition = null;
            if (String.IsNullOrEmpty(searchStr3) == false)
            {
                var propertyName = SearchFields.Where(g => g.code.Contains(searchField3.Trim())).First().exp.Trim(); // "GerbRod.N3"

                FilterBuilder fb = new FilterBuilder();
                condition = fb.Build(member, propertyName, searchStr3, searchStrOr3, searchStrOr33);
                var l = Expression.Lambda<Func<maindb, bool>>(condition, param);
                maindb = maindb.Where(l); // если нужен тип IEnumerable, то gerbs.Where(l.Compile());

                TempData["searchField3"] = searchField3;
                TempData["searchStr3"] = searchStr3; // запомню поисковую строку
                TempData["searchStrOr3"] = searchStrOr3; // запомню поисковую строку
                TempData["searchStrOr33"] = searchStrOr33; // запомню поисковую строку
            }

            condition = null;
            if (String.IsNullOrEmpty(searchStr4) == false)
            {
                var propertyName = SearchFields.Where(g => g.code.Contains(searchField4.Trim())).First().exp.Trim(); // "GerbRod.N3"

                FilterBuilder fb = new FilterBuilder();
                condition = fb.Build(member, propertyName, searchStr4, searchStrOr4, searchStrOr44);
                var l = Expression.Lambda<Func<maindb, bool>>(condition, param);
                maindb = maindb.Where(l); // если нужен тип IEnumerable, то gerbs.Where(l.Compile());

                TempData["searchField4"] = searchField4;
                TempData["searchStr4"] = searchStr4; // запомню поисковую строку
                TempData["searchStrOr4"] = searchStrOr4; // запомню поисковую строку
                TempData["searchStrOr44"] = searchStrOr44; // запомню поисковую строку

            }

            condition = null;
            if (String.IsNullOrEmpty(searchStr5) == false)
            {
                var propertyName = SearchFields.Where(g => g.code.Contains(searchField5.Trim())).First().exp.Trim(); // "GerbRod.N3"

                FilterBuilder fb = new FilterBuilder();
                condition = fb.Build(member, propertyName, searchStr5, searchStrOr5, searchStrOr55);
                var l = Expression.Lambda<Func<maindb, bool>>(condition, param);
                maindb = maindb.Where(l); // если нужен тип IEnumerable, то gerbs.Where(l.Compile());

                TempData["searchField5"] = searchField5;
                TempData["searchStr5"] = searchStr5; // запомню поисковую строку
                TempData["searchStrOr5"] = searchStrOr5; // запомню поисковую строку
                TempData["searchStrOr55"] = searchStrOr55; // запомню поисковую строку

            } 


            ViewBag.countRecords = maindb.Count();

            // выбрать первые 200 записей max_records
            //max_records_total = Request.Form["txtMaxRec"];ПОЧЕМУ так не пришло?

            //if (String.IsNullOrEmpty(max_records_total)) ПОЧЕМУ так не пришло?
             if (String.IsNullOrEmpty(txtMaxRec))
             {
                ViewBag.str_max_records = "200";
             }
          
             if (Int32.TryParse(txtMaxRec, out max_records)) // StringToInt
             {
                 maindb = maindb.Take(max_records);
             }
             else
             {
                 maindb = maindb.Take(200);
             }
             
           

            switch (sortOrder)
            {
                case "name_desc":
                    maindb = maindb.OrderByDescending(m => m.accename_eng);
                    break;
                case "name_asc":
                    maindb = maindb.OrderBy(m => m.accename_eng);
                    break;
                case "name_rus_desc":
                    maindb = maindb.OrderByDescending(m => m.accename_rus);
                    break;
                case "name_rus_asc":
                    maindb = maindb.OrderBy(m => m.accename_rus);
                    break;
                case "accenumb_desc":
                    maindb = maindb.OrderByDescending(m => m.accenumb);
                    break;
                case "accenumb_asc":
                    maindb = maindb.OrderBy(m => m.accenumb);
                    break;

                case "coord_desc":
                    maindb = maindb.OrderByDescending(m => m.latitude);
                    break;
                case "coord_asc":
                    maindb = maindb.OrderBy(m => m.latitude);
                    break;

                case "crop_desc":
                    maindb = maindb.OrderByDescending(m => m.botanic.taxonomy_crop.crop_name_eng);
                    break;
                case "crop_asc":
                    maindb = maindb.OrderBy(m => m.botanic.taxonomy_crop.crop_name_eng);
                    break;

                case "Date":
                    maindb = maindb.OrderBy(m => m.acqdate);
                    break;
                case "date_desc":
                    maindb = maindb.OrderByDescending(m => m.acqdate);
                    break;
                default:
                    maindb = maindb.OrderBy(m => m.accenumb);
                    break;
            }


            // vovkor закомментировал ToList и добавил ToPagedList
            //return View(maindb.ToList());
            int pageSize = 20; // количество строк на странице
            int pageNumber = (page ?? 1);
            return View(maindb.ToPagedList(pageNumber, pageSize));
        }

        // GET: /maindb/Details/5  *******************************************************************************************
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            maindb maindb = db.maindb.Find(id);
            if (maindb == null)
            {
                return HttpNotFound();
            }
            TempData["strFromEdit"] = "1"; // чтобы Index знал, что пришли из Edit
            return View(maindb);
        }


        // vovkor добавил функции GetGenus, GetGenusList для dropdownlist
        //       public class GenusList
        //       {
        //           public int id;
        //           public string genus_name;
        //       }
        //       private List<GenusList> GetGenusList(int id_family) // 
        //       {
        //           //List<GenusList> result = new  List<GenusList>();
        //           var values_genus = (from gn in db.taxonomy_genus
        //                                           where gn.family_code == id_family
        //                               select new GenusList {
        //                                                id = gn.id,
        //                                                genus_name = gn.genus_name
        //                                                + ((gn.genus_authority != null) ? " " + gn.genus_authority : null)
        //                                                + ((gn.subgenus_name != null) ? ", " + gn.subgenus_name : null)
        //                                                + ((gn.section_name != null) ? ", " + gn.section_name : null)
        //                                                + ((gn.subsection_name != null) ? ", " + gn.subsection_name : null)
        //                                                + ((gn.series_name != null) ? ", " + gn.series_name : null)
        //                                                + ((gn.subseries_name != null) ? ", " + gn.subseries_name : null)
        //                               }).ToList<GenusList>();
        //           //return result;
        //           return values_genus;

        //       }
        //
        public JsonResult GetGenus(int id)
        {
            // ViewBag.taxon_genus = new SelectList(db.taxonomy_genus.Where(gn => gn.family_code == taxonomy_g.family_code), "id", "genus_name", botan.genus_code); // botan.genus_code выбранное значение
            //return Json(new SelectList(db.taxonomy_genus.Where(gn => gn.family_code == id), "id", "genus_name"));
            // ХОТЕЛ ЗАМЕНИТЬ НА ВЫЗОВ ФУНКЦИИ GetGenusList !!! потому что этот кусок используется 2 раза. НЕ ПОЛУЧИЛОСЬ!!!
            var values_genus = (from gn in db.taxonomy_genus
                                where gn.family_code == id
                                select new
                                {
                                    id = gn.id,
                                    genus_name = gn.genus_name
                                    + ((gn.genus_authority != null) ? " " + gn.genus_authority : null)
                                    + ((gn.subgenus_name != null) ? ", " + gn.subgenus_name : null)
                                    + ((gn.section_name != null) ? ", " + gn.section_name : null)
                                    + ((gn.subsection_name != null) ? ", " + gn.subsection_name : null)
                                    + ((gn.series_name != null) ? ", " + gn.series_name : null)
                                    + ((gn.subseries_name != null) ? ", " + gn.subseries_name : null)
                                }).ToList();

            return Json(new SelectList(values_genus, "id", "genus_name"));
        }

        // vovkor добавил функцию GetGenus для dropdownlist
        public JsonResult GetSpecies(int id)
        {
            //return Json(new SelectList(db.botanic.Where(btn => btn.genus_code == id), "Code", "species"));
            // ХОТЕЛ ЗАМЕНИТЬ НА ВЫЗОВ ФУНКЦИИ!!! потому что этот кусок используется 2 раза. НЕ ПОЛУЧИЛОСЬ!!!
            var values_botanic = (from bn in db.botanic
                                  where bn.genus_code == id
                                  select new
                                  {
                                      Code = bn.Code,
                                      species = bn.Code + ": " + bn.species
                                      + ((bn.species_author != null) ? " " + bn.species_author : null)
                                      + ((bn.subspecies != null) ? ", " + bn.subspecies : null)
                                      + ((bn.grp_variety != null) ? ", " + bn.grp_variety : null)
                                      + ((bn.variety != null) ? ", " + bn.variety : null)
                                      + ((bn.subvariety != null) ? ", " + bn.subvariety : null)
                                      + ((bn.forma != null) ? ", " + bn.forma : null)
                                  }).ToList();

            return Json(new SelectList(values_botanic, "Code", "species"));
        }

        // vovkor добавил функцию GetRegion для dropdownlist
        public JsonResult GetRegion(int id)
        {
            var values_region = (from geo in db.geography
                                 where Math.Truncate((double)(geo.GGP / 100)) == Math.Truncate((double)(id / 100))
                                 select new
                                 {
                                     GGP = geo.GGP,
                                     GEORUS = geo.GEORUS
                                 }).ToList();

            return Json(new SelectList(values_region, "GGP", "GEORUS"));
        }

        // vovkor добавил функцию GetInstitut для dropdownlist
        public JsonResult GetInstitut(int id)
        {
            var values_institut = (from inst in db.INSTITUT
                                   where Math.Truncate((double)(inst.GEOCOD / 100)) == Math.Truncate((double)(id / 100))
                                   select new
                                   {
                                       NA = inst.NA,
                                       NAMEINST = inst.NAMEINST
                                   }).ToList();

            return Json(new SelectList(values_institut, "NA", "NAMEINST")); //GEOCOD
        }

        public JsonResult AutocompleteExpedition(string term) // экспедиции // AND = &&      OR = ||
        {
            var values_expedition = db.KRIS_EXP.Where(a => (a.STT.ToUpper().Contains(term.ToUpper())
                                                         || a.SUP.ToUpper().Contains(term.ToUpper())
                                                         || a.GOD.ToString().Contains(term)
                                                         )
                                                         && term.Length > 3)
                                        .Select(a => new { value = a.NUM_EXP, label = ((a.STT != null) ? a.STT : null) + ((a.SUP != null) ? " " + a.SUP : null) + ((a.GOD != null) ? " " + a.GOD : null) }); // a.SUP}); // для autocomplete Требуются label и value
            //.Distinct();
            return Json(values_expedition, JsonRequestBehavior.AllowGet);

        }

        // Поиск, автозаполнение
        public JsonResult AutocompleteSearchStr(string term, string sField) // Виды // AND = &&      OR = ||
        {
            IQueryable values_main = null; // Select возвращает тип IQueryable

            var param = Expression.Parameter(typeof(maindb), "m"); // Откуда выбираем данные
            Expression member = param;

            Expression condition = null;
            if (String.IsNullOrEmpty(term) == false)
            {
                var propertyName = SearchFields.Where(g => g.code.Contains(sField.Trim())).First().exp.Trim(); // "GerbRod.N3"
                // пользовательский тип FilterBuilder
                FilterBuilder fb = new FilterBuilder();
                condition = fb.Build1(member, propertyName, term);
                var l = Expression.Lambda<Func<maindb, bool>>(condition, param);  //m => m.GerbRod.N3.ToLower().Contains("1") OR  m.GerbRod.N3.ToLower().Contains("2")
                //Expression<Func<gerb, bool>> 
                //l.Compile(); // вернёт тип Func<> 

                // Если where принимает аргумент типа Func<>, возвращает тип IEnumerable, а gerbs у нас IQueyable поэтому надо передавать Expression Func<>
                //maindb = maindb.Where(l); // если нужен тип IEnumerable, то gerbs.Where(l.Compile());
                //gerbs = gerbs.Where(l.Compile()); // так не компилируется 
                // запрос 
                // https://www.codeproject.com/Articles/1079028/Build-Lambda-Expressions-Dynamically
                // https://stackoverflow.com/questions/16516971/linq-dynamic-select
                // Формируем Select(a => new { id = a.accenumb, value = a.accenumb, label = a.accenumb })
                // AutoCompleteData - Куда отдаем данные
                var xNew = Expression.New(typeof(AutoCompleteData)); // функция new для AutoCompleteData{...}
                var fields = "id,value,label";
                var bindings = fields.Split(',').Select( o => o.Trim())
                    .Select( o => {
                    var miACData = typeof(AutoCompleteData).GetProperty(o);
                    //var mi = typeof(maindb).GetProperty(propertyName);
                    // Expression.Property method does not support nested properties
                    //var xOriginal = Expression.PropertyOrField(param, propertyName);  //var xOriginal = Expression.Property(param, mi); // MemberExpression (m.accenumb)
                    var xOriginal = propertyName.Split('.').Aggregate((Expression)param, Expression.Property);
                   
                    return Expression.Bind(miACData, xOriginal);
                  }
                );
                var xInit = Expression.MemberInit(xNew, bindings);
                var lambda = Expression.Lambda<Func<maindb, AutoCompleteData>>(xInit, param);
                
                values_main = db.maindb.Where(l //Where(a => (a.species.ToUpper().Contains(term.ToUpper())
                    )
                   // .Select(a => new { id = a.accenumb, value = a.accenumb, label = a.accenumb }).Distinct().Take(20); // для autocomplete Требуются label и value
                   // или
                   // .Select(a => new { id = a.botanic.taxonomy_genus.genus_name, value = a.botanic.taxonomy_genus.genus_name, label = a.botanic.taxonomy_genus.genus_name }).Distinct().Take(20);
                   
                   .Select(lambda).Distinct().Take(20);
            }

            return Json(values_main, JsonRequestBehavior.AllowGet);

        }
        // GET: /maindb/Create   *******************************************************************************************
        public ActionResult Create()
        {
         /* ViewBag.botanic_code = new SelectList(db.botanic, "Code", "species");
            ViewBag.collsrc = new SelectList(db.collsrc, "code", "name_rus");
            ViewBag.doncty = new SelectList(db.geography, "GGP", "GEORUS");
            ViewBag.oricode = new SelectList(db.geography, "GGP", "GEORUS");
            ViewBag.donor = new SelectList(db.INSTITUT, "NA", "NAMEINST");
            ViewBag.collcode = new SelectList(db.INSTITUT, "NA", "NAMEINST");
            ViewBag.bredcode = new SelectList(db.INSTITUT, "NA", "NAMEINST");
            ViewBag.expedition = new SelectList(db.KRIS_EXP, "NUM_EXP", "ACR"); */

            // vovkor найдем выбранные записи в справочнике родов (genus), видов (botanic)
            botanic botan = db.botanic.Find(50000000); // ищет по primary key, возвращается объект DataRow // unknown 
            taxonomy_genus taxonomy_g = db.taxonomy_genus.Find(botan.genus_code);

            if (!(botan == null))
            {
                // vovkor для genus делаем SelectList
                var values_genus = (from gn in db.taxonomy_genus
                                    //where gn.id == 308 // unknown
                                    where gn.family_code == taxonomy_g.family_code
                                    select new
                                    {
                                        id = gn.id,
                                        genus_name = gn.genus_name
                                        + ((gn.genus_authority != null) ? " " + gn.genus_authority : null)
                                        + ((gn.subgenus_name != null) ? ", " + gn.subgenus_name : null)
                                        + ((gn.section_name != null) ? ", " + gn.section_name : null)
                                        + ((gn.subsection_name != null) ? ", " + gn.subsection_name : null)
                                        + ((gn.series_name != null) ? ", " + gn.series_name : null)
                                        + ((gn.subseries_name != null) ? ", " + gn.subseries_name : null)
                                    }).ToList();

                ViewBag.taxon_genus = new SelectList(values_genus, "id", "genus_name");
            }

            if (!(taxonomy_g == null))
            {
                var values_family = (from fam in db.taxonomy_family
                                     //where fam.id == 80 // unknown
                                     select new { id = fam.id, family_name = fam.family_name + ((fam.family_authority != null) ? " " + fam.family_authority : null) }).ToList();
                ViewBag.taxon_family = new SelectList(values_family, "id", "family_name");
            }

            var values_botanic = (from bn in db.botanic
                                  // where bn.Code == 50000000 // unknown
                                  where bn.genus_code == botan.genus_code
                                  select new
                                  {
                                      Code = bn.Code,
                                      species = bn.Code + ": " + bn.species
                                      + ((bn.species_author != null) ? " " + bn.species_author : null)
                                      + ((bn.subspecies != null) ? ", " + bn.subspecies : null)
                                      + ((bn.grp_variety != null) ? ", " + bn.grp_variety : null)
                                      + ((bn.variety != null) ? ", " + bn.variety : null)
                                      + ((bn.subvariety != null) ? ", " + bn.subvariety : null)
                                      + ((bn.forma != null) ? ", " + bn.forma : null)
                                  }).ToList();
            ViewBag.botanic_code = new SelectList(values_botanic, "Code", "species");

            ViewBag.collsrc = new SelectList(db.collsrc, "code", "name_rus");

            
            string doncty_country;
            //doncty_country_code = 999999; 
            doncty_country = "999999"; // unknown

            ViewBag.doncty_country = new SelectList(db.geography.Where(geo => geo.GGP.ToString().Substring(4).Equals("00")).OrderBy(geo => geo.GEORUS), "GGP", "GEORUS");//// substring отчитывает от 0
            ViewBag.doncty = new SelectList(db.geography.Where(geo => geo.GGP.ToString().Substring(0, 4).Equals(doncty_country)), "GGP", "GEORUS");

            
            string oricode_country;
            //oricode_country_code = 999999; 
            oricode_country = "999999"; // unknown

            ViewBag.oricode_country = new SelectList(db.geography.Where(geo => geo.GGP.ToString().Substring(4).Equals("00")).OrderBy(geo => geo.GEORUS), "GGP", "GEORUS");//// substring отчитывает от 0
            ViewBag.oricode = new SelectList(db.geography.Where(geo => geo.GGP.ToString().Substring(0, 4).Equals(oricode_country)), "GGP", "GEORUS");

            // Институты
            // список все стран
            ViewBag.donor_country = new SelectList(db.geography.Where(geo => geo.GGP.ToString().Substring(4).Equals("00")), "GGP", "GEORUS");//// substring отчитывает от 0
            ViewBag.donor = new SelectList(db.INSTITUT.Where(i => i.GEOCOD == null), "NA", "NAMEINST"); // пустой

            // список все стран
            ViewBag.collcode_country = new SelectList(db.geography.Where(geo => geo.GGP.ToString().Substring(4).Equals("00")), "GGP", "GEORUS");//// substring отчитывает от 0
            ViewBag.collcode = new SelectList(db.INSTITUT.Where(i => i.GEOCOD == null), "NA", "NAMEINST"); // пустой
            
            // список всех стран
            ViewBag.bredcode_country = new SelectList(db.geography.Where(geo => geo.GGP.ToString().Substring(4).Equals("00")), "GGP", "GEORUS");//// substring отчитывает от 0
            ViewBag.bredcode = new SelectList(db.INSTITUT.Where(i => i.GEOCOD == null), "NA", "NAMEINST"); // пустой
            
            // Институты END
            // Экспедиции (выпадающий список меняем на поле с автозаполнением)

            ViewBag.liffom = new SelectList(db.liffom, "form_code", "fullname");
            ViewBag.sampstat = new SelectList(db.sampstat_full, "sampstat", "fullname");
            ViewBag.storage = new SelectList(db.storage, "code", "fullname");
            ViewBag.owner_str = User.Identity.GetUserName();
            //ViewBag.acqdate = DateTime.Now;
            return View();
        }

        // POST: /maindb/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include="id,accenumb,collnumb,botanic_code,accename_eng,accename_rus,acqdate,doncty,oricode,collsite_eng,collsite_rus,colldate,liffom,sampstat,dbkey,nintrod,nexped,nother,ndonor,donor,duplsite,storage,pedigree_rus,pedigree_eng,collcode,bredcode,latitude,longitude,elevation,collsrc,expedition,REMARKS,dostupen,date_reseed,owner_str")] maindb maindb)
        {
            if (ModelState.IsValid)
            {
                db.maindb.Add(maindb);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.botanic_code = new SelectList(db.botanic, "Code", "species", maindb.botanic_code);
            ViewBag.collsrc = new SelectList(db.collsrc, "code", "name_rus", maindb.collsrc);
            ViewBag.doncty = new SelectList(db.geography, "GGP", "GEORUS", maindb.doncty);
            ViewBag.oricode = new SelectList(db.geography, "GGP", "GEORUS", maindb.oricode);
            ViewBag.donor = new SelectList(db.INSTITUT, "NA", "NAMEINST", maindb.donor);
            ViewBag.collcode = new SelectList(db.INSTITUT, "NA", "NAMEINST", maindb.collcode);
            ViewBag.bredcode = new SelectList(db.INSTITUT, "NA", "NAMEINST", maindb.bredcode);
            ViewBag.expedition = new SelectList(db.KRIS_EXP, "NUM_EXP", "ACR", maindb.expedition);
            ViewBag.liffom = new SelectList(db.liffom, "form_code", "fullname", maindb.liffom);
            ViewBag.sampstat = new SelectList(db.sampstat_full, "sampstat", "fullname", maindb.sampstat);
            ViewBag.storage = new SelectList(db.storage, "code", "fullname", maindb.storage);
            return View(maindb);
        }




        // GET: /maindb/Edit/5   *******************************************************************************************
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            maindb maindb = db.maindb.Find(id);
            if (maindb == null)
            {
                return HttpNotFound();
            }

            // vovkor найдем выбранные записи в справочнике родов (genus), видов (botanic)
            botanic botan = db.botanic.Find(maindb.botanic_code); // ищет по primary key, возвращается объект DataRow
            taxonomy_genus taxonomy_g = db.taxonomy_genus.Find(botan.genus_code);

            // vovkor для genus делаем SelectList
            if (!(botan == null))
            {
                var values_genus = (from gn in db.taxonomy_genus where gn.family_code == taxonomy_g.family_code 
                                    select new { id = gn.id,
                                                 genus_name = gn.genus_name
                                                 + ((gn.genus_authority != null) ? " " + gn.genus_authority : null)
                                                 + ((gn.subgenus_name != null) ? ", " + gn.subgenus_name : null)
                                                 + ((gn.section_name != null) ? ", " + gn.section_name : null)
                                                 + ((gn.subsection_name != null) ? ", " + gn.subsection_name : null)
                                                 + ((gn.series_name != null) ? ", " + gn.series_name : null)
                                                 + ((gn.subseries_name != null) ? ", " + gn.subseries_name : null)
                                               }).ToList();
                //ViewBag.taxon_genus = new SelectList(db.taxonomy_genus.Where(gn => gn.family_code == taxonomy_g.family_code), "id", "genus_name", botan.genus_code); // botan.genus_code выбранное значение
                ViewBag.taxon_genus = new SelectList(values_genus, "id", "genus_name", botan.genus_code); // botan.genus_code выбранное значение
            }

            // vovkor для family делаем SelectList
            if (!(taxonomy_g == null))
            {
                var values_family = (from fam in db.taxonomy_family select new { id = fam.id, family_name = fam.family_name + ((fam.family_authority != null) ? " "+fam.family_authority : null)}).ToList();
                //ViewBag.taxon_family = new SelectList(db.taxonomy_family, "id", "family_name", taxonomy_g.family_code); // taxonomy_g.family_code выбранное значение
                ViewBag.taxon_family = new SelectList(values_family, "id", "family_name", taxonomy_g.family_code); // taxonomy_g.family_code выбранное значение
            }

            // vovkor добавил SelectList для family и genus ([items for list], [data value field], [data text field], [selected value])
            //ViewBag.botanic_code = new SelectList(db.botanic.Where(btn => btn.genus_code == botan.genus_code), "Code", "species", maindb.botanic_code); // ViewBag.botanic_code = new SelectList(db.botanic, "Code", "species", maindb.botanic_code);
            var values_botanic = (from bn in db.botanic
                                  where bn.genus_code == botan.genus_code
                                select new
                                {
                                    Code = bn.Code,
                                    species = bn.Code + ": " + bn.species
                                    + ((bn.species_author != null) ? " " + bn.species_author : null)
                                    + ((bn.subspecies != null) ? ", " + bn.subspecies : null)
                                    + ((bn.grp_variety != null) ? ", " + bn.grp_variety : null)
                                    + ((bn.variety != null) ? ", " + bn.variety : null)
                                    + ((bn.subvariety != null) ? ", " + bn.subvariety : null)
                                    + ((bn.forma != null) ? ", " + bn.forma : null)
                                }).ToList();
            ViewBag.botanic_code = new SelectList(values_botanic, "Code", "species", maindb.botanic_code); // ViewBag.botanic_code = new SelectList(db.botanic, "Code", "species", maindb.botanic_code);

            
            ViewBag.collsrc = new SelectList(db.collsrc, "code", "name_rus", maindb.collsrc);
            int doncty_country_code;
            string doncty_country;
            if (!(maindb.doncty == null))
            {
                doncty_country_code = Convert.ToInt32(Convert.ToString(maindb.doncty).Substring(0, 4) + "00"); // если последние две цифры = "00", то это код страны
                doncty_country = Convert.ToString(maindb.doncty).Substring(0, 4); // код страны без конечных "00"
            } 
            else
            {
                doncty_country_code = 999999; doncty_country = "999999";
            } //unknown по-умолчанию;
            
            
            ViewBag.doncty_country = new SelectList(db.geography.Where(geo => geo.GGP.ToString().Substring(4).Equals("00")).OrderBy(geo => geo.GEORUS), "GGP", "GEORUS", doncty_country_code);//// substring отчитывает от 0
            ViewBag.doncty = new SelectList(db.geography.Where(geo => geo.GGP.ToString().Substring(0, 4).Equals(doncty_country)), "GGP", "GEORUS", maindb.doncty);

            int oricode_country_code;
            string oricode_country;
            if (!(maindb.oricode == null))
            {
                oricode_country_code = Convert.ToInt32(Convert.ToString(maindb.oricode).Substring(0, 4) + "00"); // если последние две цифры = "00", то это код страны 
                oricode_country = Convert.ToString(maindb.oricode).Substring(0, 4); // код страны без конечных "00"
            } 
            else
            {
                oricode_country_code = 999999; oricode_country = "999999";
            } //unknown по-умолчанию;
            
            
            ViewBag.oricode_country = new SelectList(db.geography.Where(geo => geo.GGP.ToString().Substring(4).Equals("00")).OrderBy(geo => geo.GEORUS), "GGP", "GEORUS", oricode_country_code);//// substring отчитывает от 0
            ViewBag.oricode = new SelectList(db.geography.Where(geo => geo.GGP.ToString().Substring(0, 4).Equals(oricode_country)), "GGP", "GEORUS", maindb.oricode);

            // Институты
            if (!(maindb.donor == null))
            {
                INSTITUT institut_donor = db.INSTITUT.Find(maindb.donor);
                if (!(institut_donor == null))
                {
                    string institut_donor_country = institut_donor.GEOCOD.Value.ToString().Substring(0, 4) + "00";
                    // список все стран
                    ViewBag.donor_country = new SelectList(db.geography.Where(geo => geo.GGP.ToString().Substring(4).Equals("00")), "GGP", "GEORUS", institut_donor_country);//// substring отчитывает от 0
                    ViewBag.donor = new SelectList(db.INSTITUT.Where(i => i.GEOCOD.ToString().Substring(0, 4).Equals(institut_donor_country.Substring(0, 4))), "NA", "NAMEINST", maindb.donor);
                }
             }
            else
            {
                // список все стран
                ViewBag.donor_country = new SelectList(db.geography.Where(geo => geo.GGP.ToString().Substring(4).Equals("00")), "GGP", "GEORUS");//// substring отчитывает от 0
                ViewBag.donor = new SelectList(db.INSTITUT.Where(i => i.GEOCOD == null), "NA", "NAMEINST"); // пустой
            }

            if (!(maindb.collcode == null))
            {
                INSTITUT institut_collcode = db.INSTITUT.Find(maindb.collcode);
                if (!(institut_collcode == null))
                {                    
                    string institut_collcode_country = institut_collcode.GEOCOD.Value.ToString().Substring(0, 4) + "00";
                    // список все стран
                    ViewBag.collcode_country = new SelectList(db.geography.Where(geo => geo.GGP.ToString().Substring(4).Equals("00")), "GGP", "GEORUS", institut_collcode_country);//// substring отчитывает от 0
                    ViewBag.collcode = new SelectList(db.INSTITUT.Where(i => i.GEOCOD.ToString().Substring(0, 4).Equals(institut_collcode_country.Substring(0, 4))), "NA", "NAMEINST", maindb.collcode);
                }
            }
            else
            {
                // список все стран
                ViewBag.collcode_country = new SelectList(db.geography.Where(geo => geo.GGP.ToString().Substring(4).Equals("00")), "GGP", "GEORUS");//// substring отчитывает от 0
                ViewBag.collcode = new SelectList(db.INSTITUT.Where(i => i.GEOCOD == null), "NA", "NAMEINST"); // пустой
            }
            if (!(maindb.bredcode == null))
            {

                INSTITUT institut_bredcode = db.INSTITUT.Find(maindb.bredcode);
                if (!(institut_bredcode == null))
                {
                    string institut_bredcode_country = institut_bredcode.GEOCOD.Value.ToString().Substring(0, 4) + "00";
                    // список всех стран
                    ViewBag.bredcode_country = new SelectList(db.geography.Where(geo => geo.GGP.ToString().Substring(4).Equals("00")), "GGP", "GEORUS", institut_bredcode_country);//// substring отчитывает от 0
                    ViewBag.bredcode = new SelectList(db.INSTITUT.Where(i => i.GEOCOD.ToString().Substring(0, 4).Equals(institut_bredcode_country.Substring(0, 4))), "NA", "NAMEINST", maindb.bredcode);
                }
            }
            else
            {
                // список всех стран
                ViewBag.bredcode_country = new SelectList(db.geography.Where(geo => geo.GGP.ToString().Substring(4).Equals("00")), "GGP", "GEORUS");//// substring отчитывает от 0
                ViewBag.bredcode = new SelectList(db.INSTITUT.Where(i => i.GEOCOD == null), "NA", "NAMEINST"); // пустой
            }
            // Институты END
            // Экспедиции (выпадающий список меняем на поле с автозаполнением)
            ViewBag.expedition = maindb.expedition; //new SelectList(db.KRIS_EXP, "NUM_EXP", "SUP", maindb.expedition); //db.KRIS_EXP.Where(i => i.NUM_EXP.Equals("1") 
            // расшифровка экспедиции
            if (!(maindb.expedition == null))
            {
                KRIS_EXP exped_cursor = db.KRIS_EXP.Find(maindb.expedition);
                if (!(exped_cursor == null))
                { ViewBag.expedition_txt = ((exped_cursor.STT != null) ? exped_cursor.STT : null) + ((exped_cursor.SUP != null) ? ", " + exped_cursor.SUP : null) + ((exped_cursor.GOD != null) ? ", " + exped_cursor.GOD : null); }
            }       

            

            ViewBag.liffom = new SelectList(db.liffom, "form_code", "fullname", maindb.liffom);
            ViewBag.sampstat = new SelectList(db.sampstat_full, "sampstat", "fullname", maindb.sampstat);
            ViewBag.storage = new SelectList(db.storage, "code", "fullname", maindb.storage);

            // добавляем логов (см. таблицу audit)
            // создаю maindb_old для audit
            // TempData keeps the information for the time of an HTTP Request
            maindb maindb_old = new maindb();
            maindb_old = maindb;
            TempData["maindb_old"] = maindb_old;
            return View(maindb);
        }
        //  VAV делегат:
        public delegate void DStr(string str1, string str2, string str3);

        // POST: /maindb/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,accenumb,collnumb,botanic_code,accename_eng,accename_rus,acqdate,doncty,oricode,collsite_eng,collsite_rus,colldate,liffom,sampstat,dbkey,nintrod,nexped,nother,ndonor,donor,duplsite,storage,pedigree_rus,pedigree_eng,collcode,bredcode,latitude,longitude,elevation,collsrc,expedition,REMARKS,dostupen,date_reseed,owner_str")] maindb maindb)
        {
            //  VAV делегат:
            //Func <string> getUserName = () =>
            //{
            //    return User.Identity.GetUserName();
            //};

            DStr AddAud = (string field_name_, string old_val, string new_val) =>
            {
                audit aud1 = new audit
                {
                    action_date = DateTime.Now
                    , owner = User.Identity.GetUserName()
                    , old_value = old_val 
                    , new_value = new_val
                    , field_name = field_name_//"accename_eng"
                    , acc_id = maindb.id
                    , accenumb = maindb.accenumb
                };
                db.audit.Add(aud1);
            };

            if (ModelState.IsValid)
            {
                
                    
                // добавляем логи в таблицу audit
                maindb maindb_old = TempData["maindb_old"] as maindb;

                if (maindb_old.accename_eng != maindb.accename_eng)
                {
                    AddAud("accename_eng", maindb_old.accename_eng, maindb.accename_eng);
                }

                if (maindb_old.accename_rus != maindb.accename_rus)
                {
                    AddAud("accename_rus", maindb_old.accename_rus, maindb.accename_rus);
                }

                if (maindb_old.accenumb != maindb.accenumb)
                {
                    AddAud("accession number", maindb_old.accenumb, maindb.accenumb);
                }
                if (maindb_old.collnumb != maindb.collnumb)
                {
                    AddAud("collecting number", maindb_old.collnumb, maindb.collnumb);
                }
                
                if (maindb_old.botanic_code != maindb.botanic_code)
                {
                    AddAud("taxonomy", maindb_old.botanic_code.ToString(), maindb.botanic_code.ToString());
                }

                if (maindb_old.acqdate != maindb.acqdate)
                {
                    AddAud("acquisition date", maindb_old.acqdate.ToString(), maindb.acqdate.ToString());
                }
                if (maindb_old.colldate != maindb.colldate)
                {
                    AddAud("collecting date", maindb_old.colldate.ToString(), maindb.colldate.ToString());
                }
                if (maindb_old.date_reseed != maindb.date_reseed)
                {
                    AddAud("дата пересева", maindb_old.date_reseed.ToString(), maindb.date_reseed.ToString());
                }
                if (maindb_old.collcode != maindb.collcode)
                {
                    AddAud("collecting institute", maindb_old.collcode, maindb.collcode);
                }
                if (maindb_old.bredcode != maindb.bredcode)
                {
                    AddAud("breeding institute", maindb_old.bredcode, maindb.bredcode);
                }
                if (maindb_old.donor != maindb.donor)
                {
                    AddAud("donor institute", maindb_old.donor, maindb.donor);
                }
                if (maindb_old.doncty != maindb.doncty)
                {
                    AddAud("donor country", maindb_old.doncty.ToString(), maindb.doncty.ToString());
                }
                if (maindb_old.expedition != maindb.expedition)
                {
                    AddAud("expedition", maindb_old.expedition, maindb.expedition);
                }
                if (maindb_old.oricode != maindb.oricode)
                {
                    AddAud("country of origin", maindb_old.oricode.ToString(), maindb.oricode.ToString());
                }
                if (maindb_old.collsite_eng != maindb.collsite_eng)
                {
                    AddAud("location of collection site (eng)", maindb_old.collsite_eng, maindb.collsite_eng);
                }
                if (maindb_old.collsite_rus != maindb.collsite_rus)
                {
                    AddAud("location of collection site (rus)", maindb_old.collsite_rus, maindb.collsite_rus);
                }
                if (maindb_old.liffom != maindb.liffom)
                {
                    AddAud("form of life", maindb_old.liffom, maindb.liffom);
                }
                if (maindb_old.sampstat != maindb.sampstat)
                {
                    AddAud("biological status of accession", maindb_old.sampstat.ToString(), maindb.sampstat.ToString());
                }
                if (maindb_old.collsrc != maindb.collsrc)
                {
                    AddAud("collection/acquisition source", maindb_old.collsrc.ToString(), maindb.collsrc.ToString());
                }
                if (maindb_old.storage != maindb.storage)
                {
                    AddAud("type of gerplasm storage", maindb_old.storage.ToString(), maindb.storage.ToString());
                }
                if (maindb_old.nintrod != maindb.nintrod)
                {
                    AddAud("introduction number", maindb_old.nintrod, maindb.nintrod);
                }
                if (maindb_old.nexped != maindb.nexped)
                {
                    AddAud("expedition number", maindb_old.nexped, maindb.nexped);
                }
                if (maindb_old.ndonor != maindb.ndonor)
                {
                    AddAud("donor accession number", maindb_old.ndonor, maindb.ndonor);
                }
                if (maindb_old.duplsite != maindb.duplsite)
                {
                    AddAud("location of safety duplicates", maindb_old.duplsite, maindb.duplsite);
                }
                if (maindb_old.pedigree_rus != maindb.pedigree_rus)
                {
                    AddAud("ancestral data (rus)", maindb_old.pedigree_rus, maindb.pedigree_rus);
                }
                if (maindb_old.pedigree_eng != maindb.pedigree_eng)
                {
                    AddAud("ancestral data (eng)", maindb_old.pedigree_eng, maindb.pedigree_eng);
                }
                if (maindb_old.latitude != maindb.latitude)
                {
                    AddAud("latitude", maindb_old.latitude.ToString(), maindb.latitude.ToString());
                }
                if (maindb_old.longitude != maindb.longitude)
                {
                    AddAud("longitude", maindb_old.longitude.ToString(), maindb.longitude.ToString());
                }
                if (maindb_old.dostupen != maindb.dostupen)
                {
                    AddAud("Доступность", maindb_old.dostupen.ToString(), maindb.dostupen.ToString());
                }
                if (maindb_old.owner_str != maindb.owner_str)
                {
                    AddAud("Владелец", maindb_old.owner_str, maindb.owner_str);
                }


                db.Entry(maindb).State = EntityState.Modified;
                db.SaveChanges();
                TempData["strFromEdit"] = "1"; // чтобы Index знал, что пришли из Edit


                return RedirectToAction("Index");
            }
            ViewBag.botanic_code = new SelectList(db.botanic, "Code", "species", maindb.botanic_code);
            ViewBag.collsrc = new SelectList(db.collsrc, "code", "name_rus", maindb.collsrc);
            ViewBag.doncty = new SelectList(db.geography, "GGP", "GEORUS", maindb.doncty);
            ViewBag.oricode = new SelectList(db.geography, "GGP", "GEORUS", maindb.oricode);
            ViewBag.donor = new SelectList(db.INSTITUT, "NA", "NAMEINST", maindb.donor);
            ViewBag.collcode = new SelectList(db.INSTITUT, "NA", "NAMEINST", maindb.collcode);
            ViewBag.bredcode = new SelectList(db.INSTITUT, "NA", "NAMEINST", maindb.bredcode);
            ViewBag.expedition = new SelectList(db.KRIS_EXP, "NUM_EXP", "ACR", maindb.expedition);
            ViewBag.liffom = new SelectList(db.liffom, "form_code", "fullname", maindb.liffom);
            ViewBag.sampstat = new SelectList(db.sampstat_full, "sampstat", "fullname", maindb.sampstat);
            ViewBag.storage = new SelectList(db.storage, "code", "fullname", maindb.storage);
            return View(maindb);
        }

        // GET: /maindb/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            maindb maindb = db.maindb.Find(id);
            if (maindb == null)
            {
                return HttpNotFound();
            }
            return View(maindb);
        }

        // POST: /maindb/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            maindb maindb = db.maindb.Find(id);

            audit aud_del = new audit
            {
                action_date = DateTime.Now
                ,
                owner = User.Identity.GetUserName()
                ,
                old_value = maindb.accename_eng
                ,
                new_value = maindb.accename_rus
                ,
                field_name = "RECORD DELETED"
                ,
                acc_id = maindb.id
                ,
                accenumb = maindb.accenumb
            };
            db.audit.Add(aud_del);

            db.maindb.Remove(maindb);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
