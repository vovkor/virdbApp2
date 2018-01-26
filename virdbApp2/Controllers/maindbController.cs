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
namespace virdbApp2.Controllers
{
    public class maindbController : Controller
    {
        private virdb2Entities db = new virdb2Entities();

        // GET: /maindb/
        // правлю Index
        //public ActionResult Index()

  
        public ActionResult Index(string sortOrder, string currentFilter, string searchString, int? page)
        {
            // вставка 1 начало
            bool dition = false; //Andrei
           
            // добавил сортировку
            ViewBag.CurrentSort = sortOrder;
            // Зачем String.IsNullOrEmpty()  ???
            //ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "name_asc";
            ViewBag.NameSortParm = sortOrder == "name_asc" ? "name_desc" : "name_asc";
            ViewBag.NameRusSortParm = sortOrder == "name_rus_asc" ? "name_rus_desc" : "name_rus_asc";
            ViewBag.AccenumbSortParm = sortOrder == "accenumb_asc" ? "accenumb_desc" : "accenumb_asc";
            ViewBag.DateSortParm = sortOrder == "Date" ? "date_desc" : "Date";
            ViewBag.CoordSortParm = sortOrder == "coord_asc" ? "coord_desc" : "coord_asc";
            
            // для PagedList
            if (searchString != null)
            {
                page = 1;
                dition = true; //Andrei

            }
            else
            {
                searchString = currentFilter;
            }

            ViewBag.CurrentFilter = searchString;
            // вставка 1 конец
            // считаю количество записей в запросе vovkor 21.02.2015
            ViewBag.countRecords = db.maindb.Include(m => m.botanic).Include(m => m.collsrc1).Include(m => m.geography).Include(m => m.geography1).Include(m => m.INSTITUT).Include(m => m.INSTITUT1).Include(m => m.INSTITUT2).Include(m => m.KRIS_EXP).Include(m => m.liffom1).Include(m => m.sampstat_full).Include(m => m.storage1).Count();
            
            // выбираем первые 200 записей ?
            var maindb = db.maindb.Include(m => m.botanic).Include(m => m.collsrc1).Include(m => m.geography).Include(m => m.geography1).Include(m => m.INSTITUT).Include(m => m.INSTITUT1).Include(m => m.INSTITUT2).Include(m => m.KRIS_EXP).Include(m => m.liffom1).Include(m => m.sampstat_full).Include(m => m.storage1);//.Take(200);
            //  VAV 20.02.2015:
            var t_usr = User.Identity.GetUserName();
            if (t_usr != "") // может использовать HasValue или String.IsNullOrEmpty ?
            {
                // считаю количество записей в запросе vovkor 21.02.2015    
                ViewBag.countRecords = maindb.Where(m => m.owner_str.Equals(t_usr)).Count();
                maindb = maindb.Where(m => m.owner_str.Equals(t_usr));//.Take(200);
            }
            
          
            // поиск

            if (!String.IsNullOrEmpty(searchString))
            {
                if (dition) //Andrei поиск - простой поиск - разбиваем строку и проводим поиск
                {
                    string[] split2 = searchString.Split(new Char[] { ' ' });
                    foreach (string s in split2)
                    {
                        if (s.Trim() != "")
                            maindb = maindb.Where(
                                                    m => (m.accenumb.ToUpper().Contains(s.ToUpper())
                                                        || m.accename_eng.ToUpper().Contains(s.ToUpper())
                                                        || m.accename_rus.ToUpper().Contains(s.ToUpper())
                                                        || m.acqdate.ToString().Contains(s)
                                                        || m.botanic.taxonomy_crop.crop_name_eng.ToUpper().Contains(s.ToUpper())
                                                        || m.botanic.taxonomy_crop.crop_name_rus.ToUpper().Contains(s.ToUpper())
                                                        || m.botanic.taxonomy_genus.taxonomy_family.family_name.ToUpper().Contains(s.ToUpper())
                                                        || m.botanic.taxonomy_genus.genus_name.ToUpper().Contains(s.ToUpper())
                                                        || m.botanic.species.ToUpper().Contains(s.ToUpper())
                                                        || m.botanic.species.ToUpper().Contains(s.ToUpper())
                                                        || m.collsite_eng.ToUpper().Contains(s.ToUpper())
                                                        || m.collsite_rus.ToUpper().Contains(s.ToUpper())
                                                        || m.geography.GEO_ENG.ToUpper().Contains(s.ToUpper())
                                                        || m.geography.GEORUS.ToUpper().Contains(s.ToUpper())
                                                        )
                                                        //  VAV 21.02.2015 - если авторизован, то фильтруем по пользователю:
                                                    && (((t_usr != "") && m.owner_str.Equals(t_usr))
                                                        || (t_usr == "")
                                                        )

                                                   );
                    }

                }
                else
                {
                    maindb = maindb.Where(
                                            m => (m.accenumb.ToUpper().Contains(searchString.ToUpper())
                                                || m.accename_eng.ToUpper().Contains(searchString.ToUpper())
                                                || m.accename_rus.ToUpper().Contains(searchString.ToUpper())
                                                || m.acqdate.ToString().Contains(searchString)
                                                || m.botanic.taxonomy_crop.crop_name_eng.ToUpper().Contains(searchString.ToUpper())
                                                || m.botanic.taxonomy_crop.crop_name_rus.ToUpper().Contains(searchString.ToUpper())
                                                || m.botanic.taxonomy_genus.taxonomy_family.family_name.ToUpper().Contains(searchString.ToUpper())
                                                || m.botanic.taxonomy_genus.genus_name.ToUpper().Contains(searchString.ToUpper())
                                                || m.botanic.species.ToUpper().Contains(searchString.ToUpper())
                                                || m.collsite_eng.ToUpper().Contains(searchString.ToUpper())
                                                || m.collsite_rus.ToUpper().Contains(searchString.ToUpper())
                                                || m.geography.GEO_ENG.ToUpper().Contains(searchString.ToUpper())
                                                || m.geography.GEORUS.ToUpper().Contains(searchString.ToUpper())
                                                )
                                                //  VAV 21.02.2015 - если авторизован, то фильтруем по пользователю:
                                            && (((t_usr != "") && m.owner_str.Equals(t_usr))
                                                || (t_usr == "")
                                                )

                                          );
                } //Andrei конец поиска
                ViewBag.countRecords = maindb.Count();
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
