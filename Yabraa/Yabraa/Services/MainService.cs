using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1.Ocsp;
using System.ComponentModel;
using Twilio.TwiML.Voice;
using Yabraa.DTOs;
using YabraaEF;
using YabraaEF.Models;
using static NuGet.Packaging.PackagingConstants;

namespace Yabraa.Services
{
    public class MainService
    {
        private ApplicationDbContext _dbContext;
        private readonly IConfiguration _configuration;
        public MainService( ApplicationDbContext dbContext, IConfiguration configuration)
        {
           
            _dbContext = dbContext;
            _configuration = configuration;
        }
        public List<ServiceTypeDTO> GetServiceTypes()
        {
          return _dbContext.ServiceTypes.Where(c => c.Enable).Select(c=> new ServiceTypeDTO { ServiceTypeId = c.ServiceTypeId , Name=c.Name,Description=c.Description }).ToList();
        }
        public MainServiceDto GetServicesDetails(int serviceTypeId)
        {
            string url = _configuration.GetValue<string>("AdminUrl");
            var services = _dbContext.Services.Include("Categories.Packages").Where(c => !c.Deleted && c.ServiceTypeId == serviceTypeId).ToList();
            var parentContainChildsIds = services.Where(c =>  c.ParentServiceId != null ).Select(c=>c.ParentServiceId).ToList();

          var oneDimensional = services.Where(c=>  c.ParentServiceId == null && !parentContainChildsIds.Contains(c.ServiceId)).ToList();
          
            MainServiceDto mainServiceDto = new MainServiceDto();
            List<ServiceDto> serviceDtos = new List<ServiceDto>();
            ServiceDto serviceDto = null; 
            foreach (var item in oneDimensional)
            {
                serviceDto = new ServiceDto()
                {
                    DetailsAR = item.DetailsAR,
                    DetailsEN = item.DetailsEN,
                    ImagePath =  $"{url}/{item.ImagePath}",
                    NameAR = item.NameAR,
                    NameEN = item.NameEN,
                    ServiceId = item.ServiceId,
                    Filters = item.Categories.Select(c=> new CategoryDto { NameEN = c.NameEN,NameAR =c.NameAR,FilterId=c.CategoryId,ServiceId = item.ServiceId}).ToList()
                };
                foreach (var filter in serviceDto.Filters)
                {
                    var Category = item.Categories.FirstOrDefault(c => !c.Deleted && c.CategoryId == filter.FilterId);
                    if (Category is not null)
                    {
                        filter.Packages = Category.Packages.Where(c=> !c.Deleted).Select(c => new PackageDto { 
                            DetailsAR = c.DetailsAR,
                            DetailsEN = c.DetailsEN,
                            ImagePath = $"{url}/{item.ImagePath}",
                            InstructionAR =c.InstructionAR,
                            InstructionEN =c.InstructionEN,
                            NameAR =c.NameAR,
                            NameEN=c.NameEN,
                            PackageId=c.PackageId,
                             Price=c.Price,
                             SubTitleAR=c.SubTitleAR,
                             SubTitleEN=c.SubTitleEN,
                             ServiceId=item.ServiceId,
                             FilterId = c.CategoryId
                        }).ToList();

                    }
                }
                serviceDtos.Add(serviceDto);
                serviceDto = null;
            }
            mainServiceDto.OneDimensionalService = serviceDtos;

            var twoDimensionalservice = services.Where(c => parentContainChildsIds.Contains(c.ServiceId)).ToList();
            foreach (var itemService in twoDimensionalservice)
            {
               // var twoDimensional = services.Where(c => c.ParentServiceId.HasValue && c.ParentServiceId == itemService.ServiceId).ToList();
                List<TwoDimensionalServiceDto> TwoDimensionalServiceDtos = new List<TwoDimensionalServiceDto>();
                TwoDimensionalServiceDto twoDimensionalServiceDto = null;
                //foreach (var item in twoDimensional)
                //{
                    var childServices = services.Where(c => c.ParentServiceId.HasValue && c.ParentServiceId.Value == itemService.ServiceId).ToList();
                    twoDimensionalServiceDto = new TwoDimensionalServiceDto();
                    if (childServices.Count == 2)
                    {
                        twoDimensionalServiceDto.ParentService = new ServiceDto()
                        {
                            DetailsAR = itemService.DetailsAR,
                            DetailsEN = itemService.DetailsEN,
                            ImagePath = $"{url}/{itemService.ImagePath}",
                            NameAR = itemService.NameAR,
                            NameEN = itemService.NameEN,
                            ServiceId = itemService.ServiceId
                        };
                        var firstChild = childServices.ElementAt(0);
                        if (firstChild is not null)
                        {
                            serviceDto = new ServiceDto()
                            {
                                DetailsAR = firstChild.DetailsAR,
                                DetailsEN = firstChild.DetailsEN,
                                //ImagePath = $"{url}/{firstChild.ImagePath}",
                                NameAR = firstChild.NameAR,
                                NameEN = firstChild.NameEN,
                                ServiceId = firstChild.ServiceId,
                                ParentServiceId = firstChild.ParentServiceId,
                                Filters = firstChild.Categories.Select(c => new CategoryDto { NameEN = c.NameEN, NameAR = c.NameAR, FilterId = c.CategoryId,ServiceId = firstChild.ServiceId,ParentServiceId= itemService.ServiceId }).ToList()
                            };
                            foreach (var filter in serviceDto.Filters)
                            {
                                // var Category1 = item.Categories.FirstOrDefault(c => c.CategoryId == filter.FilterId);

                                var Category = firstChild.Categories.FirstOrDefault(c => c.CategoryId == filter.FilterId);

                                if (Category is not null)
                                {
                                    filter.Packages = Category.Packages.Select(c => new PackageDto
                                    {
                                        DetailsAR = c.DetailsAR,
                                        DetailsEN = c.DetailsEN,
                                        ImagePath =  $"{url}/{c.ImagePath}",
                                        InstructionAR = c.InstructionAR,
                                        InstructionEN = c.InstructionEN,
                                        NameAR = c.NameAR,
                                        NameEN = c.NameEN,
                                        PackageId = c.PackageId,
                                        Price = c.Price,
                                        SubTitleAR = c.SubTitleAR,
                                        SubTitleEN = c.SubTitleEN,
                                        FilterId = filter.FilterId,
                                        ServiceId = firstChild.ServiceId,
                                    }).ToList();

                                }
                            }
                            twoDimensionalServiceDto.FirstService = serviceDto;
                            serviceDto = null;

                        }

                        var secondChild = childServices.ElementAt(1);
                        if (secondChild is not null)
                        {
                            serviceDto = new ServiceDto()
                            {
                                DetailsAR = secondChild.DetailsAR,
                                DetailsEN = secondChild.DetailsEN,
                                //ImagePath = $"{url}/{secondChild.ImagePath}",
                                NameAR = secondChild.NameAR,
                                NameEN = secondChild.NameEN,
                                ServiceId = secondChild.ServiceId,
                                ParentServiceId  = secondChild.ParentServiceId,
                                Filters = secondChild.Categories.Select(c => new CategoryDto { NameEN = c.NameEN, NameAR = c.NameAR, FilterId = c.CategoryId, ServiceId = secondChild.ServiceId, ParentServiceId = itemService.ServiceId }).ToList()
                            };
                            foreach (var filter in serviceDto.Filters)
                            {
                                var Category = secondChild.Categories.FirstOrDefault(c => c.CategoryId == filter.FilterId);
                                if (Category is not null)
                                {
                                    filter.Packages = Category.Packages.Select(c => new PackageDto
                                    {
                                        DetailsAR = c.DetailsAR,
                                        DetailsEN = c.DetailsEN,
                                        ImagePath = $"{url}/{c.ImagePath}" ,
                                        InstructionAR = c.InstructionAR,
                                        InstructionEN = c.InstructionEN,
                                        NameAR = c.NameAR,
                                        NameEN = c.NameEN,
                                        PackageId = c.PackageId,
                                        Price = c.Price,
                                        SubTitleAR = c.SubTitleAR,
                                        SubTitleEN = c.SubTitleEN,
                                        FilterId = filter.FilterId,
                                        ServiceId = secondChild.ServiceId,
                                    }).ToList();

                                }
                            }
                            twoDimensionalServiceDto.SecondService = serviceDto;
                            serviceDto = null;

                        }
                        TwoDimensionalServiceDtos.Add(twoDimensionalServiceDto);
                    }
                  
               // }
                mainServiceDto.TwoDimensionalService = TwoDimensionalServiceDtos;
            }

          //  var twoDimensional = services.Where(c => c.ParentServiceId.HasValue && parentContainChildsIds.Contains(c.ParentServiceId)).ToList();
            
           
            return mainServiceDto;
        }
        public void InsertData()
        {
            YabraaEF.Models.Service Nursing = new YabraaEF.Models.Service()
            {
                CreateDT = DateTime.Now,
                CreateSystemUserId = "1",
                Deleted = false,
                DetailsAR = "مجموعة من التحليلات لتقييم صحةالجسم ويساعد في الكشف عن المشاكل الصحيةوأسبابها",
                DetailsEN = "A set of analyzes to assess the health of the body and help in detecting health problems and their causes",
                NameAR = "خدمات التمريض المنزلي",
                NameEN = "Nursing services",
                ImagePath = "servicesImages/home-care-resources.png"
            };
            YabraaEF.Models.Service Children = new YabraaEF.Models.Service()
            {
                CreateDT = DateTime.Now,
                CreateSystemUserId = "1",
                Deleted = false,
                DetailsAR = "مجموعة من التحليلات لتقييم صحةالجسم ويساعد في الكشف عن المشاكل الصحيةوأسبابها",
                DetailsEN = "A set of analyzes to assess the health of the body and help in detecting health problems and their causes",
                NameAR = "صحة الأطفال",
                NameEN = "Children Health",
                ImagePath = "servicesImages/7-things-to-do.jpg"
            };
            YabraaEF.Models.Service Women = new YabraaEF.Models.Service()
            {
                CreateDT = DateTime.Now,
                CreateSystemUserId = "1",
                Deleted = false,
                DetailsAR = "مجموعة من التحليلات لتقييم صحةالجسم ويساعد في الكشف عن المشاكل الصحيةوأسبابها",
                DetailsEN = "A set of analyzes to assess the health of the body and help in detecting health problems and their causes",
                NameAR = "صحة المرأة",
                NameEN = "Wome’s Health",
                ImagePath = "servicesImages/IMG-20191215-WA0003.jpg"
            };
            YabraaEF.Models.Service Natural = new YabraaEF.Models.Service()
            {
                CreateDT = DateTime.Now,
                CreateSystemUserId = "1",
                Deleted = false,
                DetailsAR = "مجموعة من التحليلات لتقييم صحةالجسم ويساعد في الكشف عن المشاكل الصحيةوأسبابها",
                DetailsEN = "A set of analyzes to assess the health of the body and help in detecting health problems and their causes",
                NameAR = "العلاج الطبيعي ",
                NameEN = "Natural Therapy",
                ImagePath = "servicesImages/homecare-services-header-image.jpg"
            };
            YabraaEF.Models.Service Doctor = new YabraaEF.Models.Service()
            {
                CreateDT = DateTime.Now,
                CreateSystemUserId = "1",
                Deleted = false,
                DetailsAR = "مجموعة من التحليلات لتقييم صحةالجسم ويساعد في الكشف عن المشاكل الصحيةوأسبابها",
                DetailsEN = "A set of analyzes to assess the health of the body and help in detecting health problems and their causes",
                NameAR = "زيارات الطبيب ",
                NameEN = "Doctor Visits",
                ImagePath = "servicesImages/managing-parents-finances.jpg"
            };
            _dbContext.Services.Add(Nursing);
            _dbContext.Services.Add(Doctor);
            _dbContext.Services.Add(Natural);
            _dbContext.Services.Add(Women);
            _dbContext.Services.Add(Children);

            YabraaEF.Models.Service Laboratory = new YabraaEF.Models.Service()
            {
                CreateDT = DateTime.Now,
                CreateSystemUserId = "1",
                Deleted = false,
                DetailsAR = "مجموعة من التحليلات لتقييم صحةالجسم ويساعد في الكشف عن المشاكل الصحيةوأسبابها",
                DetailsEN = "A set of analyzes to assess the health of the body and help in detecting health problems and their causes",
                NameAR = "التحاليل المخبرية",
                NameEN = "Laboratory analyzes",
                ImagePath = "servicesImages/paying-for-home-care.jpg"
            };

            _dbContext.Services.Add(Laboratory);
            _dbContext.SaveChanges();
            YabraaEF.Models.Service analyzesa = new YabraaEF.Models.Service()
            {
                CreateDT = DateTime.Now,
                CreateSystemUserId = "1",
                Deleted = false,
                //DetailsAR = "مجموعة من التحليلات لتقييم صحةالجسم ويساعد في الكشف عن المشاكل الصحيةوأسبابها",
                //DetailsEN = "A set of analyzes to assess the health of the body and help in detecting health problems and their causes",
                NameAR = "أ - تحليل",
                NameEN = "(A) analyzes ",
                //ImagePath = "servicesImages/paying-for-home-care.jpg"
                ParentServiceId = Laboratory.ServiceId
            };
            YabraaEF.Models.Service analyzesb = new YabraaEF.Models.Service()
            {
                CreateDT = DateTime.Now,
                CreateSystemUserId = "1",
                Deleted = false,
                //DetailsAR = "مجموعة من التحليلات لتقييم صحةالجسم ويساعد في الكشف عن المشاكل الصحيةوأسبابها",
                //DetailsEN = "A set of analyzes to assess the health of the body and help in detecting health problems and their causes",
                NameAR = "ب - تحليل",
                NameEN = "(B) analyzes ",
                //ImagePath = "servicesImages/paying-for-home-care.jpg"
                ParentServiceId = Laboratory.ServiceId
            };
            _dbContext.Services.Add(analyzesa);
            _dbContext.Services.Add(analyzesb);
            _dbContext.SaveChanges();


            Category categoryNatural1 = new Category()
            {
                CreateDT = DateTime.Now,
                ServiceId = Natural.ServiceId,
                NameAR = "العلاج الاول",
                NameEN = "I treatment"

            };
            Category categoryNatural2 = new Category()
            {
                CreateDT = DateTime.Now,
                ServiceId = Natural.ServiceId,
                NameAR = "العلاج الثاني",
                NameEN = "II treatment"

            };
            Category categoryNatural3 = new Category()
            {
                CreateDT = DateTime.Now,
                ServiceId = Natural.ServiceId,
                NameAR = "العلاج الثالث",
                NameEN = "III treatment"

            };
            Category categoryNatural4 = new Category()
            {
                CreateDT = DateTime.Now,
                ServiceId = Natural.ServiceId,
                NameAR = "العلاج الرابع",
                NameEN = "IV treatment"

            };
            Category categoryNatural5 = new Category()
            {
                CreateDT = DateTime.Now,
                ServiceId = Natural.ServiceId,
                NameAR = "العلاج الخامس",
                NameEN = "IV treatment"

            };
            _dbContext.Categories.Add(categoryNatural1);
            _dbContext.Categories.Add(categoryNatural2);
            _dbContext.Categories.Add(categoryNatural3);
            _dbContext.Categories.Add(categoryNatural4);
            _dbContext.Categories.Add(categoryNatural5);


            Category categoryNursing1 = new Category()
            {
                CreateDT = DateTime.Now,
                ServiceId = Nursing.ServiceId,
                NameAR = "العلاج الاول",
                NameEN = "I treatment"

            };
            Category categoryNursing2 = new Category()
            {
                CreateDT = DateTime.Now,
                ServiceId = Nursing.ServiceId,
                NameAR = "العلاج الثاني",
                NameEN = "II treatment"

            };
            Category categoryNursing3 = new Category()
            {
                CreateDT = DateTime.Now,
                ServiceId = Nursing.ServiceId,
                NameAR = "العلاج الثالث",
                NameEN = "III treatment"

            };
            Category categoryNursing4 = new Category()
            {
                CreateDT = DateTime.Now,
                ServiceId = Nursing.ServiceId,
                NameAR = "العلاج الرابع",
                NameEN = "IV treatment"

            };
            Category categoryNursing5 = new Category()
            {
                CreateDT = DateTime.Now,
                ServiceId = Nursing.ServiceId,
                NameAR = "العلاج الخامس",
                NameEN = "IV treatment"

            };
            _dbContext.Categories.Add(categoryNursing1);
            _dbContext.Categories.Add(categoryNursing2);
            _dbContext.Categories.Add(categoryNursing3);
            _dbContext.Categories.Add(categoryNursing4);
            _dbContext.Categories.Add(categoryNursing5);

            Category categoryChildren1 = new Category()
            {
                CreateDT = DateTime.Now,
                ServiceId = Children.ServiceId,
                NameAR = "العلاج الاول",
                NameEN = "I treatment"

            };
            Category categoryChildren2 = new Category()
            {
                CreateDT = DateTime.Now,
                ServiceId = Children.ServiceId,
                NameAR = "العلاج الثاني",
                NameEN = "II treatment"

            };
            Category categoryChildren3 = new Category()
            {
                CreateDT = DateTime.Now,
                ServiceId = Children.ServiceId,
                NameAR = "العلاج الثالث",
                NameEN = "III treatment"

            };
            Category categoryChildren4 = new Category()
            {
                CreateDT = DateTime.Now,
                ServiceId = Children.ServiceId,
                NameAR = "العلاج الرابع",
                NameEN = "IV treatment"

            };
            Category categoryChildren5 = new Category()
            {
                CreateDT = DateTime.Now,
                ServiceId = Children.ServiceId,
                NameAR = "العلاج الخامس",
                NameEN = "IV treatment"

            };
            _dbContext.Categories.Add(categoryChildren1);
            _dbContext.Categories.Add(categoryChildren2);
            _dbContext.Categories.Add(categoryChildren3);
            _dbContext.Categories.Add(categoryChildren4);
            _dbContext.Categories.Add(categoryChildren5);


            Category categoryWomen1 = new Category()
            {
                CreateDT = DateTime.Now,
                ServiceId = Women.ServiceId,
                NameAR = "العلاج الاول",
                NameEN = "I treatment"

            };
            Category categoryWomen2 = new Category()
            {
                CreateDT = DateTime.Now,
                ServiceId = Women.ServiceId,
                NameAR = "العلاج الثاني",
                NameEN = "II treatment"

            };
            Category categoryWomen3 = new Category()
            {
                CreateDT = DateTime.Now,
                ServiceId = Women.ServiceId,
                NameAR = "العلاج الثالث",
                NameEN = "III treatment"

            };
            Category categoryWomen4 = new Category()
            {
                CreateDT = DateTime.Now,
                ServiceId = Women.ServiceId,
                NameAR = "العلاج الرابع",
                NameEN = "IV treatment"

            };
            Category categoryWomen5 = new Category()
            {
                CreateDT = DateTime.Now,
                ServiceId = Women.ServiceId,
                NameAR = "العلاج الخامس",
                NameEN = "IV treatment"

            };
            _dbContext.Categories.Add(categoryWomen1);
            _dbContext.Categories.Add(categoryWomen2);
            _dbContext.Categories.Add(categoryWomen3);
            _dbContext.Categories.Add(categoryWomen4);
            _dbContext.Categories.Add(categoryWomen5);

            Category categoryDoctor1 = new Category()
            {
                CreateDT = DateTime.Now,
                ServiceId = Doctor.ServiceId,
                NameAR = "العلاج الاول",
                NameEN = "I treatment"

            };
            Category categoryDoctor2 = new Category()
            {
                CreateDT = DateTime.Now,
                ServiceId = Doctor.ServiceId,
                NameAR = "العلاج الثاني",
                NameEN = "II treatment"

            };
            Category categoryDoctor3 = new Category()
            {
                CreateDT = DateTime.Now,
                ServiceId = Doctor.ServiceId,
                NameAR = "العلاج الثالث",
                NameEN = "III treatment"

            };
            Category categoryDoctor4 = new Category()
            {
                CreateDT = DateTime.Now,
                ServiceId = Doctor.ServiceId,
                NameAR = "العلاج الرابع",
                NameEN = "IV treatment"

            };
            Category categoryDoctor5 = new Category()
            {
                CreateDT = DateTime.Now,
                ServiceId = Doctor.ServiceId,
                NameAR = "العلاج الخامس",
                NameEN = "IV treatment"

            };
            _dbContext.Categories.Add(categoryDoctor1);
            _dbContext.Categories.Add(categoryDoctor2);
            _dbContext.Categories.Add(categoryDoctor3);
            _dbContext.Categories.Add(categoryDoctor4);
            _dbContext.Categories.Add(categoryDoctor5);


            Category categoryanalyzesa1 = new Category()
            {
                CreateDT = DateTime.Now,
                ServiceId = analyzesa.ServiceId,
                NameAR = "تحاليل اول",
                NameEN = "I analysis"

            };
            Category categoryanalyzesa2 = new Category()
            {
                CreateDT = DateTime.Now,
                ServiceId = analyzesa.ServiceId,
                NameAR = "تحاليل ثاني",
                NameEN = "II analysis"

            };
            Category categoryanalyzesa3 = new Category()
            {
                CreateDT = DateTime.Now,
                ServiceId = analyzesa.ServiceId,
                NameAR = "تحاليل لثالث",
                NameEN = "III analysis"

            };
            Category categoryanalyzesa4 = new Category()
            {
                CreateDT = DateTime.Now,
                ServiceId = analyzesa.ServiceId,
                NameAR = "تحاليل لرابع",
                NameEN = "IV analysis"

            };
            Category categoryanalyzesa5 = new Category()
            {
                CreateDT = DateTime.Now,
                ServiceId = analyzesa.ServiceId,
                NameAR = "تحاليل لخامس",
                NameEN = "IV analysis"

            };
            _dbContext.Categories.Add(categoryanalyzesa1);
            _dbContext.Categories.Add(categoryanalyzesa2);
            _dbContext.Categories.Add(categoryanalyzesa3);
            _dbContext.Categories.Add(categoryanalyzesa4);
            _dbContext.Categories.Add(categoryanalyzesa5);

            Category categoryanalyzesb1 = new Category()
            {
                CreateDT = DateTime.Now,
                ServiceId = analyzesb.ServiceId,
                NameAR = "تحاليل اول",
                NameEN = "I analysis"

            };
            Category categoryanalyzesb2 = new Category()
            {
                CreateDT = DateTime.Now,
                ServiceId = analyzesb.ServiceId,
                NameAR = "تحاليل ثاني",
                NameEN = "II analysis"

            };
            Category categoryanalyzesb3 = new Category()
            {
                CreateDT = DateTime.Now,
                ServiceId = analyzesb.ServiceId,
                NameAR = "تحاليل لثالث",
                NameEN = "III analysis"

            };
            Category categoryanalyzesb4 = new Category()
            {
                CreateDT = DateTime.Now,
                ServiceId = analyzesb.ServiceId,
                NameAR = "تحاليل لرابع",
                NameEN = "IV analysis"

            };
            Category categoryanalyzesb5 = new Category()
            {
                CreateDT = DateTime.Now,
                ServiceId = analyzesb.ServiceId,
                NameAR = "تحاليل لخامس",
                NameEN = "IV analysis"

            };
            _dbContext.Categories.Add(categoryanalyzesb1);
            _dbContext.Categories.Add(categoryanalyzesb2);
            _dbContext.Categories.Add(categoryanalyzesb3);
            _dbContext.Categories.Add(categoryanalyzesb4);
            _dbContext.Categories.Add(categoryanalyzesb5);

            _dbContext.SaveChanges();


            Package package1 = new Package()
            {
                Price = 100,
                ServiceId = Nursing.ServiceId,
                CategoryId = categoryNursing1.CategoryId,                
                CreateDT =DateTime.Now,
                CreateSystemUserId ="1",
                NameEN = "Physiotherapy for Children",
                NameAR = " العلاج الطبيعي للاطفال",
                SubTitleAR = "8 زيارات في الشهر",
                SubTitleEN = "8 visits per month",
                DetailsAR = "مجموعة من التحليلات لتقييم صحةالجسم ويساعد في الكشف عن العيوب الصحيةوأسبابها مجموعة من التحليلات لمجموعة منتحليلات لتقييم صحة الجسم وتساعد في الكشف عن المشاكل الصحية ويسبب مجموعة من التحليلات ل",
                DetailsEN= "A set of analyzes to assess the health of the body and help in detecting health pro blems and their causes A set of analyzes to A set of analyzes to assess the health of the body and help in detecting health problems and their causes A set of analyzes to ",
                InstructionAR = "يرجى إبلاغ الطبيب عن نوع الحالة المرضية والتاريخ المرضي السابق للحالة. يقوم فريقنا الطبي والتمريضي باستخدام أجهزة الفحص السريري والمؤشرات الحيوية أثناء الزيارة. تشمل خدماتنا الرعاية المنزلية لكل الأعمار ومختلف الحالات الطبية.",
                InstructionEN = "Please inform the doctor about the type of medical condition and the previous medical history of the condition. Our medical and nursing team uses clinical examination equipment and vital signs during the visit. Our services include home care for all ages and various medical conditions.",
                ImagePath = "packagesImage/Home-Solutions.jpg"

            };

            Package package2 = new Package()
            {
                Price = 150,
                ServiceId = Nursing.ServiceId,
                CategoryId = categoryNursing2.CategoryId,
                CreateDT = DateTime.Now,
                CreateSystemUserId = "1",
                NameEN = "Physiotherapy for Children",
                NameAR = " العلاج الطبيعي للاطفال",
                SubTitleAR = "8 زيارات في الشهر",
                SubTitleEN = "8 visits per month",
                DetailsAR = "مجموعة من التحليلات لتقييم صحةالجسم ويساعد في الكشف عن العيوب الصحيةوأسبابها مجموعة من التحليلات لمجموعة منتحليلات لتقييم صحة الجسم وتساعد في الكشف عن المشاكل الصحية ويسبب مجموعة من التحليلات ل",
                DetailsEN = "A set of analyzes to assess the health of the body and help in detecting health pro blems and their causes A set of analyzes to A set of analyzes to assess the health of the body and help in detecting health problems and their causes A set of analyzes to ",
                InstructionAR = "يرجى إبلاغ الطبيب عن نوع الحالة المرضية والتاريخ المرضي السابق للحالة. يقوم فريقنا الطبي والتمريضي باستخدام أجهزة الفحص السريري والمؤشرات الحيوية أثناء الزيارة. تشمل خدماتنا الرعاية المنزلية لكل الأعمار ومختلف الحالات الطبية.",
                InstructionEN = "Please inform the doctor about the type of medical condition and the previous medical history of the condition. Our medical and nursing team uses clinical examination equipment and vital signs during the visit. Our services include home care for all ages and various medical conditions.",
                ImagePath = "packagesImage/Home-Solutions.jpg"

            };
            Package package3 = new Package()
            {
                Price = 200,
                ServiceId = Nursing.ServiceId,
                CategoryId = categoryNursing2.CategoryId,
                CreateDT = DateTime.Now,
                CreateSystemUserId = "1",
                NameEN = "Physiotherapy for Children",
                NameAR = " العلاج الطبيعي للاطفال",
                SubTitleAR = "8 زيارات في الشهر",
                SubTitleEN = "8 visits per month",
                DetailsAR = "مجموعة من التحليلات لتقييم صحةالجسم ويساعد في الكشف عن العيوب الصحيةوأسبابها مجموعة من التحليلات لمجموعة منتحليلات لتقييم صحة الجسم وتساعد في الكشف عن المشاكل الصحية ويسبب مجموعة من التحليلات ل",
                DetailsEN = "A set of analyzes to assess the health of the body and help in detecting health pro blems and their causes A set of analyzes to A set of analyzes to assess the health of the body and help in detecting health problems and their causes A set of analyzes to ",
                InstructionAR = "يرجى إبلاغ الطبيب عن نوع الحالة المرضية والتاريخ المرضي السابق للحالة. يقوم فريقنا الطبي والتمريضي باستخدام أجهزة الفحص السريري والمؤشرات الحيوية أثناء الزيارة. تشمل خدماتنا الرعاية المنزلية لكل الأعمار ومختلف الحالات الطبية.",
                InstructionEN = "Please inform the doctor about the type of medical condition and the previous medical history of the condition. Our medical and nursing team uses clinical examination equipment and vital signs during the visit. Our services include home care for all ages and various medical conditions.",
                ImagePath = "packagesImage/Home-Solutions.jpg"

            };
            Package package4 = new Package()
            {
                Price = 250,
                ServiceId = Nursing.ServiceId,
                CategoryId = categoryNursing2.CategoryId,
                CreateDT = DateTime.Now,
                CreateSystemUserId = "1",
                NameEN = "Physiotherapy for Children",
                NameAR = " العلاج الطبيعي للاطفال",
                SubTitleAR = "8 زيارات في الشهر",
                SubTitleEN = "8 visits per month",
                DetailsAR = "مجموعة من التحليلات لتقييم صحةالجسم ويساعد في الكشف عن العيوب الصحيةوأسبابها مجموعة من التحليلات لمجموعة منتحليلات لتقييم صحة الجسم وتساعد في الكشف عن المشاكل الصحية ويسبب مجموعة من التحليلات ل",
                DetailsEN = "A set of analyzes to assess the health of the body and help in detecting health pro blems and their causes A set of analyzes to A set of analyzes to assess the health of the body and help in detecting health problems and their causes A set of analyzes to ",
                InstructionAR = "يرجى إبلاغ الطبيب عن نوع الحالة المرضية والتاريخ المرضي السابق للحالة. يقوم فريقنا الطبي والتمريضي باستخدام أجهزة الفحص السريري والمؤشرات الحيوية أثناء الزيارة. تشمل خدماتنا الرعاية المنزلية لكل الأعمار ومختلف الحالات الطبية.",
                InstructionEN = "Please inform the doctor about the type of medical condition and the previous medical history of the condition. Our medical and nursing team uses clinical examination equipment and vital signs during the visit. Our services include home care for all ages and various medical conditions.",
                ImagePath = "packagesImage/Home-Solutions.jpg"

            };

            Package package5 = new Package()
            {
                Price = 300,
                ServiceId = Nursing.ServiceId,
                CategoryId = categoryNursing3.CategoryId,
                CreateDT = DateTime.Now,
                CreateSystemUserId = "1",
                NameEN = "Physiotherapy for Children",
                NameAR = " العلاج الطبيعي للاطفال",
                SubTitleAR = "8 زيارات في الشهر",
                SubTitleEN = "8 visits per month",
                DetailsAR = "مجموعة من التحليلات لتقييم صحةالجسم ويساعد في الكشف عن العيوب الصحيةوأسبابها مجموعة من التحليلات لمجموعة منتحليلات لتقييم صحة الجسم وتساعد في الكشف عن المشاكل الصحية ويسبب مجموعة من التحليلات ل",
                DetailsEN = "A set of analyzes to assess the health of the body and help in detecting health pro blems and their causes A set of analyzes to A set of analyzes to assess the health of the body and help in detecting health problems and their causes A set of analyzes to ",
                InstructionAR = "يرجى إبلاغ الطبيب عن نوع الحالة المرضية والتاريخ المرضي السابق للحالة. يقوم فريقنا الطبي والتمريضي باستخدام أجهزة الفحص السريري والمؤشرات الحيوية أثناء الزيارة. تشمل خدماتنا الرعاية المنزلية لكل الأعمار ومختلف الحالات الطبية.",
                InstructionEN = "Please inform the doctor about the type of medical condition and the previous medical history of the condition. Our medical and nursing team uses clinical examination equipment and vital signs during the visit. Our services include home care for all ages and various medical conditions.",
                ImagePath = "packagesImage/Home-Solutions.jpg"

            };
            Package package6 = new Package()
            {
                Price = 350,
                ServiceId = Nursing.ServiceId,
                CategoryId = categoryNursing3.CategoryId,
                CreateDT = DateTime.Now,
                CreateSystemUserId = "1",
                NameEN = "Physiotherapy for Children",
                NameAR = " العلاج الطبيعي للاطفال",
                SubTitleAR = "8 زيارات في الشهر",
                SubTitleEN = "8 visits per month",
                DetailsAR = "مجموعة من التحليلات لتقييم صحةالجسم ويساعد في الكشف عن العيوب الصحيةوأسبابها مجموعة من التحليلات لمجموعة منتحليلات لتقييم صحة الجسم وتساعد في الكشف عن المشاكل الصحية ويسبب مجموعة من التحليلات ل",
                DetailsEN = "A set of analyzes to assess the health of the body and help in detecting health pro blems and their causes A set of analyzes to A set of analyzes to assess the health of the body and help in detecting health problems and their causes A set of analyzes to ",
                InstructionAR = "يرجى إبلاغ الطبيب عن نوع الحالة المرضية والتاريخ المرضي السابق للحالة. يقوم فريقنا الطبي والتمريضي باستخدام أجهزة الفحص السريري والمؤشرات الحيوية أثناء الزيارة. تشمل خدماتنا الرعاية المنزلية لكل الأعمار ومختلف الحالات الطبية.",
                InstructionEN = "Please inform the doctor about the type of medical condition and the previous medical history of the condition. Our medical and nursing team uses clinical examination equipment and vital signs during the visit. Our services include home care for all ages and various medical conditions.",
                ImagePath = "packagesImage/Home-Solutions.jpg"

            };

            Package package7 = new Package()
            {
                Price = 400,
                ServiceId = Nursing.ServiceId,
                CategoryId = categoryNursing4.CategoryId,
                CreateDT = DateTime.Now,
                CreateSystemUserId = "1",
                NameEN = "Physiotherapy for Children",
                NameAR = " العلاج الطبيعي للاطفال",
                SubTitleAR = "8 زيارات في الشهر",
                SubTitleEN = "8 visits per month",
                DetailsAR = "مجموعة من التحليلات لتقييم صحةالجسم ويساعد في الكشف عن العيوب الصحيةوأسبابها مجموعة من التحليلات لمجموعة منتحليلات لتقييم صحة الجسم وتساعد في الكشف عن المشاكل الصحية ويسبب مجموعة من التحليلات ل",
                DetailsEN = "A set of analyzes to assess the health of the body and help in detecting health pro blems and their causes A set of analyzes to A set of analyzes to assess the health of the body and help in detecting health problems and their causes A set of analyzes to ",
                InstructionAR = "يرجى إبلاغ الطبيب عن نوع الحالة المرضية والتاريخ المرضي السابق للحالة. يقوم فريقنا الطبي والتمريضي باستخدام أجهزة الفحص السريري والمؤشرات الحيوية أثناء الزيارة. تشمل خدماتنا الرعاية المنزلية لكل الأعمار ومختلف الحالات الطبية.",
                InstructionEN = "Please inform the doctor about the type of medical condition and the previous medical history of the condition. Our medical and nursing team uses clinical examination equipment and vital signs during the visit. Our services include home care for all ages and various medical conditions.",
                ImagePath = "packagesImage/Home-Solutions.jpg"

            };

            Package package8 = new Package()
            {
                Price = 460,
                ServiceId = Nursing.ServiceId,
                CategoryId = categoryNursing5.CategoryId,
                CreateDT = DateTime.Now,
                CreateSystemUserId = "1",
                NameEN = "Physiotherapy for Children",
                NameAR = " العلاج الطبيعي للاطفال",
                SubTitleAR = "8 زيارات في الشهر",
                SubTitleEN = "8 visits per month",
                DetailsAR = "مجموعة من التحليلات لتقييم صحةالجسم ويساعد في الكشف عن العيوب الصحيةوأسبابها مجموعة من التحليلات لمجموعة منتحليلات لتقييم صحة الجسم وتساعد في الكشف عن المشاكل الصحية ويسبب مجموعة من التحليلات ل",
                DetailsEN = "A set of analyzes to assess the health of the body and help in detecting health pro blems and their causes A set of analyzes to A set of analyzes to assess the health of the body and help in detecting health problems and their causes A set of analyzes to ",
                InstructionAR = "يرجى إبلاغ الطبيب عن نوع الحالة المرضية والتاريخ المرضي السابق للحالة. يقوم فريقنا الطبي والتمريضي باستخدام أجهزة الفحص السريري والمؤشرات الحيوية أثناء الزيارة. تشمل خدماتنا الرعاية المنزلية لكل الأعمار ومختلف الحالات الطبية.",
                InstructionEN = "Please inform the doctor about the type of medical condition and the previous medical history of the condition. Our medical and nursing team uses clinical examination equipment and vital signs during the visit. Our services include home care for all ages and various medical conditions.",
                ImagePath = "packagesImage/Home-Solutions.jpg"

            };
            Package package9 = new Package()
            {
                Price = 500,
                ServiceId = Nursing.ServiceId,
                CategoryId = categoryNursing5.CategoryId,
                CreateDT = DateTime.Now,
                CreateSystemUserId = "1",
                NameEN = "Physiotherapy for Children",
                NameAR = " العلاج الطبيعي للاطفال",
                SubTitleAR = "8 زيارات في الشهر",
                SubTitleEN = "8 visits per month",
                DetailsAR = "مجموعة من التحليلات لتقييم صحةالجسم ويساعد في الكشف عن العيوب الصحيةوأسبابها مجموعة من التحليلات لمجموعة منتحليلات لتقييم صحة الجسم وتساعد في الكشف عن المشاكل الصحية ويسبب مجموعة من التحليلات ل",
                DetailsEN = "A set of analyzes to assess the health of the body and help in detecting health pro blems and their causes A set of analyzes to A set of analyzes to assess the health of the body and help in detecting health problems and their causes A set of analyzes to ",
                InstructionAR = "يرجى إبلاغ الطبيب عن نوع الحالة المرضية والتاريخ المرضي السابق للحالة. يقوم فريقنا الطبي والتمريضي باستخدام أجهزة الفحص السريري والمؤشرات الحيوية أثناء الزيارة. تشمل خدماتنا الرعاية المنزلية لكل الأعمار ومختلف الحالات الطبية.",
                InstructionEN = "Please inform the doctor about the type of medical condition and the previous medical history of the condition. Our medical and nursing team uses clinical examination equipment and vital signs during the visit. Our services include home care for all ages and various medical conditions.",
                ImagePath = "packagesImage/Home-Solutions.jpg"

            };

            _dbContext.Packages.Add(package1);
            _dbContext.Packages.Add(package2);
            _dbContext.Packages.Add(package3);
            _dbContext.Packages.Add(package4);
            _dbContext.Packages.Add(package5);
            _dbContext.Packages.Add(package6);
            _dbContext.Packages.Add(package7);
            _dbContext.Packages.Add(package8);
            _dbContext.Packages.Add(package9);


            Package package10 = new Package()
            {
                Price = 600,
                ServiceId = Children.ServiceId,
                CategoryId = categoryChildren1.CategoryId,
                CreateDT = DateTime.Now,
                CreateSystemUserId = "1",
                NameEN = "Physiotherapy for Children",
                NameAR = " العلاج الطبيعي للاطفال",
                SubTitleAR = "8 زيارات في الشهر",
                SubTitleEN = "8 visits per month",
                DetailsAR = "مجموعة من التحليلات لتقييم صحةالجسم ويساعد في الكشف عن العيوب الصحيةوأسبابها مجموعة من التحليلات لمجموعة منتحليلات لتقييم صحة الجسم وتساعد في الكشف عن المشاكل الصحية ويسبب مجموعة من التحليلات ل",
                DetailsEN = "A set of analyzes to assess the health of the body and help in detecting health pro blems and their causes A set of analyzes to A set of analyzes to assess the health of the body and help in detecting health problems and their causes A set of analyzes to ",
                InstructionAR = "يرجى إبلاغ الطبيب عن نوع الحالة المرضية والتاريخ المرضي السابق للحالة. يقوم فريقنا الطبي والتمريضي باستخدام أجهزة الفحص السريري والمؤشرات الحيوية أثناء الزيارة. تشمل خدماتنا الرعاية المنزلية لكل الأعمار ومختلف الحالات الطبية.",
                InstructionEN = "Please inform the doctor about the type of medical condition and the previous medical history of the condition. Our medical and nursing team uses clinical examination equipment and vital signs during the visit. Our services include home care for all ages and various medical conditions.",
                ImagePath = "packagesImage/images.jpg"

            };

            Package package11 = new Package()
            {
                Price = 650,
                ServiceId = Children.ServiceId,
                CategoryId = categoryChildren2.CategoryId,
                CreateDT = DateTime.Now,
                CreateSystemUserId = "1",
                NameEN = "Physiotherapy for Children",
                NameAR = " العلاج الطبيعي للاطفال",
                SubTitleAR = "8 زيارات في الشهر",
                SubTitleEN = "8 visits per month",
                DetailsAR = "مجموعة من التحليلات لتقييم صحةالجسم ويساعد في الكشف عن العيوب الصحيةوأسبابها مجموعة من التحليلات لمجموعة منتحليلات لتقييم صحة الجسم وتساعد في الكشف عن المشاكل الصحية ويسبب مجموعة من التحليلات ل",
                DetailsEN = "A set of analyzes to assess the health of the body and help in detecting health pro blems and their causes A set of analyzes to A set of analyzes to assess the health of the body and help in detecting health problems and their causes A set of analyzes to ",
                InstructionAR = "يرجى إبلاغ الطبيب عن نوع الحالة المرضية والتاريخ المرضي السابق للحالة. يقوم فريقنا الطبي والتمريضي باستخدام أجهزة الفحص السريري والمؤشرات الحيوية أثناء الزيارة. تشمل خدماتنا الرعاية المنزلية لكل الأعمار ومختلف الحالات الطبية.",
                InstructionEN = "Please inform the doctor about the type of medical condition and the previous medical history of the condition. Our medical and nursing team uses clinical examination equipment and vital signs during the visit. Our services include home care for all ages and various medical conditions.",
                ImagePath = "packagesImage/images.jpg"

            };
            Package package12 = new Package()
            {
                Price = 700,
                ServiceId = Children.ServiceId,
                CategoryId = categoryChildren2.CategoryId,
                CreateDT = DateTime.Now,
                CreateSystemUserId = "1",
                NameEN = "Physiotherapy for Children",
                NameAR = " العلاج الطبيعي للاطفال",
                SubTitleAR = "8 زيارات في الشهر",
                SubTitleEN = "8 visits per month",
                DetailsAR = "مجموعة من التحليلات لتقييم صحةالجسم ويساعد في الكشف عن العيوب الصحيةوأسبابها مجموعة من التحليلات لمجموعة منتحليلات لتقييم صحة الجسم وتساعد في الكشف عن المشاكل الصحية ويسبب مجموعة من التحليلات ل",
                DetailsEN = "A set of analyzes to assess the health of the body and help in detecting health pro blems and their causes A set of analyzes to A set of analyzes to assess the health of the body and help in detecting health problems and their causes A set of analyzes to ",
                InstructionAR = "يرجى إبلاغ الطبيب عن نوع الحالة المرضية والتاريخ المرضي السابق للحالة. يقوم فريقنا الطبي والتمريضي باستخدام أجهزة الفحص السريري والمؤشرات الحيوية أثناء الزيارة. تشمل خدماتنا الرعاية المنزلية لكل الأعمار ومختلف الحالات الطبية.",
                InstructionEN = "Please inform the doctor about the type of medical condition and the previous medical history of the condition. Our medical and nursing team uses clinical examination equipment and vital signs during the visit. Our services include home care for all ages and various medical conditions.",
                ImagePath = "packagesImage/images.jpg"

            };
            Package package13 = new Package()
            {
                Price = 750,
                ServiceId = Children.ServiceId,
                CategoryId = categoryChildren2.CategoryId,
                CreateDT = DateTime.Now,
                CreateSystemUserId = "1",
                NameEN = "Physiotherapy for Children",
                NameAR = " العلاج الطبيعي للاطفال",
                SubTitleAR = "8 زيارات في الشهر",
                SubTitleEN = "8 visits per month",
                DetailsAR = "مجموعة من التحليلات لتقييم صحةالجسم ويساعد في الكشف عن العيوب الصحيةوأسبابها مجموعة من التحليلات لمجموعة منتحليلات لتقييم صحة الجسم وتساعد في الكشف عن المشاكل الصحية ويسبب مجموعة من التحليلات ل",
                DetailsEN = "A set of analyzes to assess the health of the body and help in detecting health pro blems and their causes A set of analyzes to A set of analyzes to assess the health of the body and help in detecting health problems and their causes A set of analyzes to ",
                InstructionAR = "يرجى إبلاغ الطبيب عن نوع الحالة المرضية والتاريخ المرضي السابق للحالة. يقوم فريقنا الطبي والتمريضي باستخدام أجهزة الفحص السريري والمؤشرات الحيوية أثناء الزيارة. تشمل خدماتنا الرعاية المنزلية لكل الأعمار ومختلف الحالات الطبية.",
                InstructionEN = "Please inform the doctor about the type of medical condition and the previous medical history of the condition. Our medical and nursing team uses clinical examination equipment and vital signs during the visit. Our services include home care for all ages and various medical conditions.",
                ImagePath = "packagesImage/images.jpg"

            };

            Package package14 = new Package()
            {
                Price = 800,
                ServiceId = Children.ServiceId,
                CategoryId = categoryChildren3.CategoryId,
                CreateDT = DateTime.Now,
                CreateSystemUserId = "1",
                NameEN = "Physiotherapy for Children",
                NameAR = " العلاج الطبيعي للاطفال",
                SubTitleAR = "8 زيارات في الشهر",
                SubTitleEN = "8 visits per month",
                DetailsAR = "مجموعة من التحليلات لتقييم صحةالجسم ويساعد في الكشف عن العيوب الصحيةوأسبابها مجموعة من التحليلات لمجموعة منتحليلات لتقييم صحة الجسم وتساعد في الكشف عن المشاكل الصحية ويسبب مجموعة من التحليلات ل",
                DetailsEN = "A set of analyzes to assess the health of the body and help in detecting health pro blems and their causes A set of analyzes to A set of analyzes to assess the health of the body and help in detecting health problems and their causes A set of analyzes to ",
                InstructionAR = "يرجى إبلاغ الطبيب عن نوع الحالة المرضية والتاريخ المرضي السابق للحالة. يقوم فريقنا الطبي والتمريضي باستخدام أجهزة الفحص السريري والمؤشرات الحيوية أثناء الزيارة. تشمل خدماتنا الرعاية المنزلية لكل الأعمار ومختلف الحالات الطبية.",
                InstructionEN = "Please inform the doctor about the type of medical condition and the previous medical history of the condition. Our medical and nursing team uses clinical examination equipment and vital signs during the visit. Our services include home care for all ages and various medical conditions.",
                ImagePath = "packagesImage/images.jpg"

            };
            Package package15 = new Package()
            {
                Price = 850,
                ServiceId = Children.ServiceId,
                CategoryId = categoryChildren3.CategoryId,
                CreateDT = DateTime.Now,
                CreateSystemUserId = "1",
                NameEN = "Physiotherapy for Children",
                NameAR = " العلاج الطبيعي للاطفال",
                SubTitleAR = "8 زيارات في الشهر",
                SubTitleEN = "8 visits per month",
                DetailsAR = "مجموعة من التحليلات لتقييم صحةالجسم ويساعد في الكشف عن العيوب الصحيةوأسبابها مجموعة من التحليلات لمجموعة منتحليلات لتقييم صحة الجسم وتساعد في الكشف عن المشاكل الصحية ويسبب مجموعة من التحليلات ل",
                DetailsEN = "A set of analyzes to assess the health of the body and help in detecting health pro blems and their causes A set of analyzes to A set of analyzes to assess the health of the body and help in detecting health problems and their causes A set of analyzes to ",
                InstructionAR = "يرجى إبلاغ الطبيب عن نوع الحالة المرضية والتاريخ المرضي السابق للحالة. يقوم فريقنا الطبي والتمريضي باستخدام أجهزة الفحص السريري والمؤشرات الحيوية أثناء الزيارة. تشمل خدماتنا الرعاية المنزلية لكل الأعمار ومختلف الحالات الطبية.",
                InstructionEN = "Please inform the doctor about the type of medical condition and the previous medical history of the condition. Our medical and nursing team uses clinical examination equipment and vital signs during the visit. Our services include home care for all ages and various medical conditions.",
                ImagePath = "packagesImage/images.jpg"

            };

            Package package16 = new Package()
            {
                Price = 900,
                ServiceId = Children.ServiceId,
                CategoryId = categoryChildren4.CategoryId,
                CreateDT = DateTime.Now,
                CreateSystemUserId = "1",
                NameEN = "Physiotherapy for Children",
                NameAR = " العلاج الطبيعي للاطفال",
                SubTitleAR = "8 زيارات في الشهر",
                SubTitleEN = "8 visits per month",
                DetailsAR = "مجموعة من التحليلات لتقييم صحةالجسم ويساعد في الكشف عن العيوب الصحيةوأسبابها مجموعة من التحليلات لمجموعة منتحليلات لتقييم صحة الجسم وتساعد في الكشف عن المشاكل الصحية ويسبب مجموعة من التحليلات ل",
                DetailsEN = "A set of analyzes to assess the health of the body and help in detecting health pro blems and their causes A set of analyzes to A set of analyzes to assess the health of the body and help in detecting health problems and their causes A set of analyzes to ",
                InstructionAR = "يرجى إبلاغ الطبيب عن نوع الحالة المرضية والتاريخ المرضي السابق للحالة. يقوم فريقنا الطبي والتمريضي باستخدام أجهزة الفحص السريري والمؤشرات الحيوية أثناء الزيارة. تشمل خدماتنا الرعاية المنزلية لكل الأعمار ومختلف الحالات الطبية.",
                InstructionEN = "Please inform the doctor about the type of medical condition and the previous medical history of the condition. Our medical and nursing team uses clinical examination equipment and vital signs during the visit. Our services include home care for all ages and various medical conditions.",
                ImagePath = "packagesImage/images.jpg"

            };

            Package package17 = new Package()
            {
                Price = 960,
                ServiceId = Children.ServiceId,
                CategoryId = categoryChildren5.CategoryId,
                CreateDT = DateTime.Now,
                CreateSystemUserId = "1",
                NameEN = "Physiotherapy for Children",
                NameAR = " العلاج الطبيعي للاطفال",
                SubTitleAR = "8 زيارات في الشهر",
                SubTitleEN = "8 visits per month",
                DetailsAR = "مجموعة من التحليلات لتقييم صحةالجسم ويساعد في الكشف عن العيوب الصحيةوأسبابها مجموعة من التحليلات لمجموعة منتحليلات لتقييم صحة الجسم وتساعد في الكشف عن المشاكل الصحية ويسبب مجموعة من التحليلات ل",
                DetailsEN = "A set of analyzes to assess the health of the body and help in detecting health pro blems and their causes A set of analyzes to A set of analyzes to assess the health of the body and help in detecting health problems and their causes A set of analyzes to ",
                InstructionAR = "يرجى إبلاغ الطبيب عن نوع الحالة المرضية والتاريخ المرضي السابق للحالة. يقوم فريقنا الطبي والتمريضي باستخدام أجهزة الفحص السريري والمؤشرات الحيوية أثناء الزيارة. تشمل خدماتنا الرعاية المنزلية لكل الأعمار ومختلف الحالات الطبية.",
                InstructionEN = "Please inform the doctor about the type of medical condition and the previous medical history of the condition. Our medical and nursing team uses clinical examination equipment and vital signs during the visit. Our services include home care for all ages and various medical conditions.",
                ImagePath = "packagesImage/images.jpg"

            };
            Package package18 = new Package()
            {
                Price = 1000,
                ServiceId = Children.ServiceId,
                CategoryId = categoryChildren5.CategoryId,
                CreateDT = DateTime.Now,
                CreateSystemUserId = "1",
                NameEN = "Physiotherapy for Children",
                NameAR = " العلاج الطبيعي للاطفال",
                SubTitleAR = "8 زيارات في الشهر",
                SubTitleEN = "8 visits per month",
                DetailsAR = "مجموعة من التحليلات لتقييم صحةالجسم ويساعد في الكشف عن العيوب الصحيةوأسبابها مجموعة من التحليلات لمجموعة منتحليلات لتقييم صحة الجسم وتساعد في الكشف عن المشاكل الصحية ويسبب مجموعة من التحليلات ل",
                DetailsEN = "A set of analyzes to assess the health of the body and help in detecting health pro blems and their causes A set of analyzes to A set of analyzes to assess the health of the body and help in detecting health problems and their causes A set of analyzes to ",
                InstructionAR = "يرجى إبلاغ الطبيب عن نوع الحالة المرضية والتاريخ المرضي السابق للحالة. يقوم فريقنا الطبي والتمريضي باستخدام أجهزة الفحص السريري والمؤشرات الحيوية أثناء الزيارة. تشمل خدماتنا الرعاية المنزلية لكل الأعمار ومختلف الحالات الطبية.",
                InstructionEN = "Please inform the doctor about the type of medical condition and the previous medical history of the condition. Our medical and nursing team uses clinical examination equipment and vital signs during the visit. Our services include home care for all ages and various medical conditions.",
                ImagePath = "packagesImage/images.jpg"

            };

            _dbContext.Packages.Add(package10);
            _dbContext.Packages.Add(package11);
            _dbContext.Packages.Add(package12);
            _dbContext.Packages.Add(package13);
            _dbContext.Packages.Add(package14);
            _dbContext.Packages.Add(package15);
            _dbContext.Packages.Add(package16);
            _dbContext.Packages.Add(package17);
            _dbContext.Packages.Add(package18);



            Package package19 = new Package()
            {
                Price = 1100,
                ServiceId = Women.ServiceId,
                CategoryId = categoryWomen1.CategoryId,
                CreateDT = DateTime.Now,
                CreateSystemUserId = "1",
                NameEN = "Physiotherapy for Children",
                NameAR = " العلاج الطبيعي للاطفال",
                SubTitleAR = "8 زيارات في الشهر",
                SubTitleEN = "8 visits per month",
                DetailsAR = "مجموعة من التحليلات لتقييم صحةالجسم ويساعد في الكشف عن العيوب الصحيةوأسبابها مجموعة من التحليلات لمجموعة منتحليلات لتقييم صحة الجسم وتساعد في الكشف عن المشاكل الصحية ويسبب مجموعة من التحليلات ل",
                DetailsEN = "A set of analyzes to assess the health of the body and help in detecting health pro blems and their causes A set of analyzes to A set of analyzes to assess the health of the body and help in detecting health problems and their causes A set of analyzes to ",
                InstructionAR = "يرجى إبلاغ الطبيب عن نوع الحالة المرضية والتاريخ المرضي السابق للحالة. يقوم فريقنا الطبي والتمريضي باستخدام أجهزة الفحص السريري والمؤشرات الحيوية أثناء الزيارة. تشمل خدماتنا الرعاية المنزلية لكل الأعمار ومختلف الحالات الطبية.",
                InstructionEN = "Please inform the doctor about the type of medical condition and the previous medical history of the condition. Our medical and nursing team uses clinical examination equipment and vital signs during the visit. Our services include home care for all ages and various medical conditions.",
                ImagePath = "packagesImage/whoweare.jpg"

            };

            Package package20 = new Package()
            {
                Price = 1200,
                ServiceId = Women.ServiceId,
                CategoryId = categoryWomen2.CategoryId,
                CreateDT = DateTime.Now,
                CreateSystemUserId = "1",
                NameEN = "Physiotherapy for Children",
                NameAR = " العلاج الطبيعي للاطفال",
                SubTitleAR = "8 زيارات في الشهر",
                SubTitleEN = "8 visits per month",
                DetailsAR = "مجموعة من التحليلات لتقييم صحةالجسم ويساعد في الكشف عن العيوب الصحيةوأسبابها مجموعة من التحليلات لمجموعة منتحليلات لتقييم صحة الجسم وتساعد في الكشف عن المشاكل الصحية ويسبب مجموعة من التحليلات ل",
                DetailsEN = "A set of analyzes to assess the health of the body and help in detecting health pro blems and their causes A set of analyzes to A set of analyzes to assess the health of the body and help in detecting health problems and their causes A set of analyzes to ",
                InstructionAR = "يرجى إبلاغ الطبيب عن نوع الحالة المرضية والتاريخ المرضي السابق للحالة. يقوم فريقنا الطبي والتمريضي باستخدام أجهزة الفحص السريري والمؤشرات الحيوية أثناء الزيارة. تشمل خدماتنا الرعاية المنزلية لكل الأعمار ومختلف الحالات الطبية.",
                InstructionEN = "Please inform the doctor about the type of medical condition and the previous medical history of the condition. Our medical and nursing team uses clinical examination equipment and vital signs during the visit. Our services include home care for all ages and various medical conditions.",
                ImagePath = "packagesImage/whoweare.jpg"

            };
            Package package21 = new Package()
            {
                Price = 1300,
                ServiceId = Women.ServiceId,
                CategoryId = categoryWomen2.CategoryId,
                CreateDT = DateTime.Now,
                CreateSystemUserId = "1",
                NameEN = "Physiotherapy for Children",
                NameAR = " العلاج الطبيعي للاطفال",
                SubTitleAR = "8 زيارات في الشهر",
                SubTitleEN = "8 visits per month",
                DetailsAR = "مجموعة من التحليلات لتقييم صحةالجسم ويساعد في الكشف عن العيوب الصحيةوأسبابها مجموعة من التحليلات لمجموعة منتحليلات لتقييم صحة الجسم وتساعد في الكشف عن المشاكل الصحية ويسبب مجموعة من التحليلات ل",
                DetailsEN = "A set of analyzes to assess the health of the body and help in detecting health pro blems and their causes A set of analyzes to A set of analyzes to assess the health of the body and help in detecting health problems and their causes A set of analyzes to ",
                InstructionAR = "يرجى إبلاغ الطبيب عن نوع الحالة المرضية والتاريخ المرضي السابق للحالة. يقوم فريقنا الطبي والتمريضي باستخدام أجهزة الفحص السريري والمؤشرات الحيوية أثناء الزيارة. تشمل خدماتنا الرعاية المنزلية لكل الأعمار ومختلف الحالات الطبية.",
                InstructionEN = "Please inform the doctor about the type of medical condition and the previous medical history of the condition. Our medical and nursing team uses clinical examination equipment and vital signs during the visit. Our services include home care for all ages and various medical conditions.",
                ImagePath = "packagesImage/whoweare.jpg"

            };
            Package package22 = new Package()
            {
                Price = 1400,
                ServiceId = Women.ServiceId,
                CategoryId = categoryWomen2.CategoryId,
                CreateDT = DateTime.Now,
                CreateSystemUserId = "1",
                NameEN = "Physiotherapy for Children",
                NameAR = " العلاج الطبيعي للاطفال",
                SubTitleAR = "8 زيارات في الشهر",
                SubTitleEN = "8 visits per month",
                DetailsAR = "مجموعة من التحليلات لتقييم صحةالجسم ويساعد في الكشف عن العيوب الصحيةوأسبابها مجموعة من التحليلات لمجموعة منتحليلات لتقييم صحة الجسم وتساعد في الكشف عن المشاكل الصحية ويسبب مجموعة من التحليلات ل",
                DetailsEN = "A set of analyzes to assess the health of the body and help in detecting health pro blems and their causes A set of analyzes to A set of analyzes to assess the health of the body and help in detecting health problems and their causes A set of analyzes to ",
                InstructionAR = "يرجى إبلاغ الطبيب عن نوع الحالة المرضية والتاريخ المرضي السابق للحالة. يقوم فريقنا الطبي والتمريضي باستخدام أجهزة الفحص السريري والمؤشرات الحيوية أثناء الزيارة. تشمل خدماتنا الرعاية المنزلية لكل الأعمار ومختلف الحالات الطبية.",
                InstructionEN = "Please inform the doctor about the type of medical condition and the previous medical history of the condition. Our medical and nursing team uses clinical examination equipment and vital signs during the visit. Our services include home care for all ages and various medical conditions.",
                ImagePath = "packagesImage/whoweare.jpg"

            };

            Package package23 = new Package()
            {
                Price = 1500,
                ServiceId = Women.ServiceId,
                CategoryId = categoryWomen3.CategoryId,
                CreateDT = DateTime.Now,
                CreateSystemUserId = "1",
                NameEN = "Physiotherapy for Children",
                NameAR = " العلاج الطبيعي للاطفال",
                SubTitleAR = "8 زيارات في الشهر",
                SubTitleEN = "8 visits per month",
                DetailsAR = "مجموعة من التحليلات لتقييم صحةالجسم ويساعد في الكشف عن العيوب الصحيةوأسبابها مجموعة من التحليلات لمجموعة منتحليلات لتقييم صحة الجسم وتساعد في الكشف عن المشاكل الصحية ويسبب مجموعة من التحليلات ل",
                DetailsEN = "A set of analyzes to assess the health of the body and help in detecting health pro blems and their causes A set of analyzes to A set of analyzes to assess the health of the body and help in detecting health problems and their causes A set of analyzes to ",
                InstructionAR = "يرجى إبلاغ الطبيب عن نوع الحالة المرضية والتاريخ المرضي السابق للحالة. يقوم فريقنا الطبي والتمريضي باستخدام أجهزة الفحص السريري والمؤشرات الحيوية أثناء الزيارة. تشمل خدماتنا الرعاية المنزلية لكل الأعمار ومختلف الحالات الطبية.",
                InstructionEN = "Please inform the doctor about the type of medical condition and the previous medical history of the condition. Our medical and nursing team uses clinical examination equipment and vital signs during the visit. Our services include home care for all ages and various medical conditions.",
                ImagePath = "packagesImage/whoweare.jpg"

            };
            Package package24 = new Package()
            {
                Price = 1600,
                ServiceId = Women.ServiceId,
                CategoryId = categoryWomen3.CategoryId,
                CreateDT = DateTime.Now,
                CreateSystemUserId = "1",
                NameEN = "Physiotherapy for Children",
                NameAR = " العلاج الطبيعي للاطفال",
                SubTitleAR = "8 زيارات في الشهر",
                SubTitleEN = "8 visits per month",
                DetailsAR = "مجموعة من التحليلات لتقييم صحةالجسم ويساعد في الكشف عن العيوب الصحيةوأسبابها مجموعة من التحليلات لمجموعة منتحليلات لتقييم صحة الجسم وتساعد في الكشف عن المشاكل الصحية ويسبب مجموعة من التحليلات ل",
                DetailsEN = "A set of analyzes to assess the health of the body and help in detecting health pro blems and their causes A set of analyzes to A set of analyzes to assess the health of the body and help in detecting health problems and their causes A set of analyzes to ",
                InstructionAR = "يرجى إبلاغ الطبيب عن نوع الحالة المرضية والتاريخ المرضي السابق للحالة. يقوم فريقنا الطبي والتمريضي باستخدام أجهزة الفحص السريري والمؤشرات الحيوية أثناء الزيارة. تشمل خدماتنا الرعاية المنزلية لكل الأعمار ومختلف الحالات الطبية.",
                InstructionEN = "Please inform the doctor about the type of medical condition and the previous medical history of the condition. Our medical and nursing team uses clinical examination equipment and vital signs during the visit. Our services include home care for all ages and various medical conditions.",
                ImagePath = "packagesImage/whoweare.jpg"

            };

            Package package25 = new Package()
            {
                Price = 1700,
                ServiceId = Women.ServiceId,
                CategoryId = categoryWomen4.CategoryId,
                CreateDT = DateTime.Now,
                CreateSystemUserId = "1",
                NameEN = "Physiotherapy for Children",
                NameAR = " العلاج الطبيعي للاطفال",
                SubTitleAR = "8 زيارات في الشهر",
                SubTitleEN = "8 visits per month",
                DetailsAR = "مجموعة من التحليلات لتقييم صحةالجسم ويساعد في الكشف عن العيوب الصحيةوأسبابها مجموعة من التحليلات لمجموعة منتحليلات لتقييم صحة الجسم وتساعد في الكشف عن المشاكل الصحية ويسبب مجموعة من التحليلات ل",
                DetailsEN = "A set of analyzes to assess the health of the body and help in detecting health pro blems and their causes A set of analyzes to A set of analyzes to assess the health of the body and help in detecting health problems and their causes A set of analyzes to ",
                InstructionAR = "يرجى إبلاغ الطبيب عن نوع الحالة المرضية والتاريخ المرضي السابق للحالة. يقوم فريقنا الطبي والتمريضي باستخدام أجهزة الفحص السريري والمؤشرات الحيوية أثناء الزيارة. تشمل خدماتنا الرعاية المنزلية لكل الأعمار ومختلف الحالات الطبية.",
                InstructionEN = "Please inform the doctor about the type of medical condition and the previous medical history of the condition. Our medical and nursing team uses clinical examination equipment and vital signs during the visit. Our services include home care for all ages and various medical conditions.",
                ImagePath = "packagesImage/whoweare.jpg"

            };

            Package package26 = new Package()
            {
                Price = 1800,
                ServiceId = Women.ServiceId,
                CategoryId = categoryWomen5.CategoryId,
                CreateDT = DateTime.Now,
                CreateSystemUserId = "1",
                NameEN = "Physiotherapy for Children",
                NameAR = " العلاج الطبيعي للاطفال",
                SubTitleAR = "8 زيارات في الشهر",
                SubTitleEN = "8 visits per month",
                DetailsAR = "مجموعة من التحليلات لتقييم صحةالجسم ويساعد في الكشف عن العيوب الصحيةوأسبابها مجموعة من التحليلات لمجموعة منتحليلات لتقييم صحة الجسم وتساعد في الكشف عن المشاكل الصحية ويسبب مجموعة من التحليلات ل",
                DetailsEN = "A set of analyzes to assess the health of the body and help in detecting health pro blems and their causes A set of analyzes to A set of analyzes to assess the health of the body and help in detecting health problems and their causes A set of analyzes to ",
                InstructionAR = "يرجى إبلاغ الطبيب عن نوع الحالة المرضية والتاريخ المرضي السابق للحالة. يقوم فريقنا الطبي والتمريضي باستخدام أجهزة الفحص السريري والمؤشرات الحيوية أثناء الزيارة. تشمل خدماتنا الرعاية المنزلية لكل الأعمار ومختلف الحالات الطبية.",
                InstructionEN = "Please inform the doctor about the type of medical condition and the previous medical history of the condition. Our medical and nursing team uses clinical examination equipment and vital signs during the visit. Our services include home care for all ages and various medical conditions.",
                ImagePath = "packagesImage/whoweare.jpg"

            };
            Package package27 = new Package()
            {
                Price = 1900,
                ServiceId = Women.ServiceId,
                CategoryId = categoryWomen5.CategoryId,
                CreateDT = DateTime.Now,
                CreateSystemUserId = "1",
                NameEN = "Physiotherapy for Children",
                NameAR = " العلاج الطبيعي للاطفال",
                SubTitleAR = "8 زيارات في الشهر",
                SubTitleEN = "8 visits per month",
                DetailsAR = "مجموعة من التحليلات لتقييم صحةالجسم ويساعد في الكشف عن العيوب الصحيةوأسبابها مجموعة من التحليلات لمجموعة منتحليلات لتقييم صحة الجسم وتساعد في الكشف عن المشاكل الصحية ويسبب مجموعة من التحليلات ل",
                DetailsEN = "A set of analyzes to assess the health of the body and help in detecting health pro blems and their causes A set of analyzes to A set of analyzes to assess the health of the body and help in detecting health problems and their causes A set of analyzes to ",
                InstructionAR = "يرجى إبلاغ الطبيب عن نوع الحالة المرضية والتاريخ المرضي السابق للحالة. يقوم فريقنا الطبي والتمريضي باستخدام أجهزة الفحص السريري والمؤشرات الحيوية أثناء الزيارة. تشمل خدماتنا الرعاية المنزلية لكل الأعمار ومختلف الحالات الطبية.",
                InstructionEN = "Please inform the doctor about the type of medical condition and the previous medical history of the condition. Our medical and nursing team uses clinical examination equipment and vital signs during the visit. Our services include home care for all ages and various medical conditions.",
                ImagePath = "packagesImage/whoweare.jpg"

            };

            _dbContext.Packages.Add(package19);
            _dbContext.Packages.Add(package20);
            _dbContext.Packages.Add(package21);
            _dbContext.Packages.Add(package22);
            _dbContext.Packages.Add(package23);
            _dbContext.Packages.Add(package24);
            _dbContext.Packages.Add(package25);
            _dbContext.Packages.Add(package26);
            _dbContext.Packages.Add(package27);


            Package package28 = new Package()
            {
                Price = 2000,
                ServiceId = Natural.ServiceId,
                CategoryId = categoryNatural1.CategoryId,
                CreateDT = DateTime.Now,
                CreateSystemUserId = "1",
                NameEN = "Physiotherapy for Children",
                NameAR = " العلاج الطبيعي للاطفال",
                SubTitleAR = "8 زيارات في الشهر",
                SubTitleEN = "8 visits per month",
                DetailsAR = "مجموعة من التحليلات لتقييم صحةالجسم ويساعد في الكشف عن العيوب الصحيةوأسبابها مجموعة من التحليلات لمجموعة منتحليلات لتقييم صحة الجسم وتساعد في الكشف عن المشاكل الصحية ويسبب مجموعة من التحليلات ل",
                DetailsEN = "A set of analyzes to assess the health of the body and help in detecting health pro blems and their causes A set of analyzes to A set of analyzes to assess the health of the body and help in detecting health problems and their causes A set of analyzes to ",
                InstructionAR = "يرجى إبلاغ الطبيب عن نوع الحالة المرضية والتاريخ المرضي السابق للحالة. يقوم فريقنا الطبي والتمريضي باستخدام أجهزة الفحص السريري والمؤشرات الحيوية أثناء الزيارة. تشمل خدماتنا الرعاية المنزلية لكل الأعمار ومختلف الحالات الطبية.",
                InstructionEN = "Please inform the doctor about the type of medical condition and the previous medical history of the condition. Our medical and nursing team uses clinical examination equipment and vital signs during the visit. Our services include home care for all ages and various medical conditions.",
                ImagePath = "packagesImage/Home-Solutions.jpg"

            };

            Package package29 = new Package()
            {
                Price = 2100,
                ServiceId = Natural.ServiceId,
                CategoryId = categoryNatural2.CategoryId,
                CreateDT = DateTime.Now,
                CreateSystemUserId = "1",
                NameEN = "Physiotherapy for Children",
                NameAR = " العلاج الطبيعي للاطفال",
                SubTitleAR = "8 زيارات في الشهر",
                SubTitleEN = "8 visits per month",
                DetailsAR = "مجموعة من التحليلات لتقييم صحةالجسم ويساعد في الكشف عن العيوب الصحيةوأسبابها مجموعة من التحليلات لمجموعة منتحليلات لتقييم صحة الجسم وتساعد في الكشف عن المشاكل الصحية ويسبب مجموعة من التحليلات ل",
                DetailsEN = "A set of analyzes to assess the health of the body and help in detecting health pro blems and their causes A set of analyzes to A set of analyzes to assess the health of the body and help in detecting health problems and their causes A set of analyzes to ",
                InstructionAR = "يرجى إبلاغ الطبيب عن نوع الحالة المرضية والتاريخ المرضي السابق للحالة. يقوم فريقنا الطبي والتمريضي باستخدام أجهزة الفحص السريري والمؤشرات الحيوية أثناء الزيارة. تشمل خدماتنا الرعاية المنزلية لكل الأعمار ومختلف الحالات الطبية.",
                InstructionEN = "Please inform the doctor about the type of medical condition and the previous medical history of the condition. Our medical and nursing team uses clinical examination equipment and vital signs during the visit. Our services include home care for all ages and various medical conditions.",
                ImagePath = "packagesImage/Home-Solutions.jpg"

            };
            Package package30 = new Package()
            {
                Price = 2300,
                ServiceId = Natural.ServiceId,
                CategoryId = categoryNatural2.CategoryId,
                CreateDT = DateTime.Now,
                CreateSystemUserId = "1",
                NameEN = "Physiotherapy for Children",
                NameAR = " العلاج الطبيعي للاطفال",
                SubTitleAR = "8 زيارات في الشهر",
                SubTitleEN = "8 visits per month",
                DetailsAR = "مجموعة من التحليلات لتقييم صحةالجسم ويساعد في الكشف عن العيوب الصحيةوأسبابها مجموعة من التحليلات لمجموعة منتحليلات لتقييم صحة الجسم وتساعد في الكشف عن المشاكل الصحية ويسبب مجموعة من التحليلات ل",
                DetailsEN = "A set of analyzes to assess the health of the body and help in detecting health pro blems and their causes A set of analyzes to A set of analyzes to assess the health of the body and help in detecting health problems and their causes A set of analyzes to ",
                InstructionAR = "يرجى إبلاغ الطبيب عن نوع الحالة المرضية والتاريخ المرضي السابق للحالة. يقوم فريقنا الطبي والتمريضي باستخدام أجهزة الفحص السريري والمؤشرات الحيوية أثناء الزيارة. تشمل خدماتنا الرعاية المنزلية لكل الأعمار ومختلف الحالات الطبية.",
                InstructionEN = "Please inform the doctor about the type of medical condition and the previous medical history of the condition. Our medical and nursing team uses clinical examination equipment and vital signs during the visit. Our services include home care for all ages and various medical conditions.",
                ImagePath = "packagesImage/Home-Solutions.jpg"

            };
            Package package31 = new Package()
            {
                Price = 2400,
                ServiceId = Natural.ServiceId,
                CategoryId = categoryNatural2.CategoryId,
                CreateDT = DateTime.Now,
                CreateSystemUserId = "1",
                NameEN = "Physiotherapy for Children",
                NameAR = " العلاج الطبيعي للاطفال",
                SubTitleAR = "8 زيارات في الشهر",
                SubTitleEN = "8 visits per month",
                DetailsAR = "مجموعة من التحليلات لتقييم صحةالجسم ويساعد في الكشف عن العيوب الصحيةوأسبابها مجموعة من التحليلات لمجموعة منتحليلات لتقييم صحة الجسم وتساعد في الكشف عن المشاكل الصحية ويسبب مجموعة من التحليلات ل",
                DetailsEN = "A set of analyzes to assess the health of the body and help in detecting health pro blems and their causes A set of analyzes to A set of analyzes to assess the health of the body and help in detecting health problems and their causes A set of analyzes to ",
                InstructionAR = "يرجى إبلاغ الطبيب عن نوع الحالة المرضية والتاريخ المرضي السابق للحالة. يقوم فريقنا الطبي والتمريضي باستخدام أجهزة الفحص السريري والمؤشرات الحيوية أثناء الزيارة. تشمل خدماتنا الرعاية المنزلية لكل الأعمار ومختلف الحالات الطبية.",
                InstructionEN = "Please inform the doctor about the type of medical condition and the previous medical history of the condition. Our medical and nursing team uses clinical examination equipment and vital signs during the visit. Our services include home care for all ages and various medical conditions.",
                ImagePath = "packagesImage/Home-Solutions.jpg"

            };

            Package package32 = new Package()
            {
                Price = 2500,
                ServiceId = Natural.ServiceId,
                CategoryId = categoryNatural3.CategoryId,
                CreateDT = DateTime.Now,
                CreateSystemUserId = "1",
                NameEN = "Physiotherapy for Children",
                NameAR = " العلاج الطبيعي للاطفال",
                SubTitleAR = "8 زيارات في الشهر",
                SubTitleEN = "8 visits per month",
                DetailsAR = "مجموعة من التحليلات لتقييم صحةالجسم ويساعد في الكشف عن العيوب الصحيةوأسبابها مجموعة من التحليلات لمجموعة منتحليلات لتقييم صحة الجسم وتساعد في الكشف عن المشاكل الصحية ويسبب مجموعة من التحليلات ل",
                DetailsEN = "A set of analyzes to assess the health of the body and help in detecting health pro blems and their causes A set of analyzes to A set of analyzes to assess the health of the body and help in detecting health problems and their causes A set of analyzes to ",
                InstructionAR = "يرجى إبلاغ الطبيب عن نوع الحالة المرضية والتاريخ المرضي السابق للحالة. يقوم فريقنا الطبي والتمريضي باستخدام أجهزة الفحص السريري والمؤشرات الحيوية أثناء الزيارة. تشمل خدماتنا الرعاية المنزلية لكل الأعمار ومختلف الحالات الطبية.",
                InstructionEN = "Please inform the doctor about the type of medical condition and the previous medical history of the condition. Our medical and nursing team uses clinical examination equipment and vital signs during the visit. Our services include home care for all ages and various medical conditions.",
                ImagePath = "packagesImage/Home-Solutions.jpg"

            };
            Package package33 = new Package()
            {
                Price = 2600,
                ServiceId = Natural.ServiceId,
                CategoryId = categoryNatural3.CategoryId,
                CreateDT = DateTime.Now,
                CreateSystemUserId = "1",
                NameEN = "Physiotherapy for Children",
                NameAR = " العلاج الطبيعي للاطفال",
                SubTitleAR = "8 زيارات في الشهر",
                SubTitleEN = "8 visits per month",
                DetailsAR = "مجموعة من التحليلات لتقييم صحةالجسم ويساعد في الكشف عن العيوب الصحيةوأسبابها مجموعة من التحليلات لمجموعة منتحليلات لتقييم صحة الجسم وتساعد في الكشف عن المشاكل الصحية ويسبب مجموعة من التحليلات ل",
                DetailsEN = "A set of analyzes to assess the health of the body and help in detecting health pro blems and their causes A set of analyzes to A set of analyzes to assess the health of the body and help in detecting health problems and their causes A set of analyzes to ",
                InstructionAR = "يرجى إبلاغ الطبيب عن نوع الحالة المرضية والتاريخ المرضي السابق للحالة. يقوم فريقنا الطبي والتمريضي باستخدام أجهزة الفحص السريري والمؤشرات الحيوية أثناء الزيارة. تشمل خدماتنا الرعاية المنزلية لكل الأعمار ومختلف الحالات الطبية.",
                InstructionEN = "Please inform the doctor about the type of medical condition and the previous medical history of the condition. Our medical and nursing team uses clinical examination equipment and vital signs during the visit. Our services include home care for all ages and various medical conditions.",
                ImagePath = "packagesImage/Home-Solutions.jpg"

            };

            Package package34 = new Package()
            {
                Price = 2700,
                ServiceId = Natural.ServiceId,
                CategoryId = categoryNatural4.CategoryId,
                CreateDT = DateTime.Now,
                CreateSystemUserId = "1",
                NameEN = "Physiotherapy for Children",
                NameAR = " العلاج الطبيعي للاطفال",
                SubTitleAR = "8 زيارات في الشهر",
                SubTitleEN = "8 visits per month",
                DetailsAR = "مجموعة من التحليلات لتقييم صحةالجسم ويساعد في الكشف عن العيوب الصحيةوأسبابها مجموعة من التحليلات لمجموعة منتحليلات لتقييم صحة الجسم وتساعد في الكشف عن المشاكل الصحية ويسبب مجموعة من التحليلات ل",
                DetailsEN = "A set of analyzes to assess the health of the body and help in detecting health pro blems and their causes A set of analyzes to A set of analyzes to assess the health of the body and help in detecting health problems and their causes A set of analyzes to ",
                InstructionAR = "يرجى إبلاغ الطبيب عن نوع الحالة المرضية والتاريخ المرضي السابق للحالة. يقوم فريقنا الطبي والتمريضي باستخدام أجهزة الفحص السريري والمؤشرات الحيوية أثناء الزيارة. تشمل خدماتنا الرعاية المنزلية لكل الأعمار ومختلف الحالات الطبية.",
                InstructionEN = "Please inform the doctor about the type of medical condition and the previous medical history of the condition. Our medical and nursing team uses clinical examination equipment and vital signs during the visit. Our services include home care for all ages and various medical conditions.",
                ImagePath = "packagesImage/Home-Solutions.jpg"

            };

            Package package35 = new Package()
            {
                Price = 2800,
                ServiceId = Natural.ServiceId,
                CategoryId = categoryNatural5.CategoryId,
                CreateDT = DateTime.Now,
                CreateSystemUserId = "1",
                NameEN = "Physiotherapy for Children",
                NameAR = " العلاج الطبيعي للاطفال",
                SubTitleAR = "8 زيارات في الشهر",
                SubTitleEN = "8 visits per month",
                DetailsAR = "مجموعة من التحليلات لتقييم صحةالجسم ويساعد في الكشف عن العيوب الصحيةوأسبابها مجموعة من التحليلات لمجموعة منتحليلات لتقييم صحة الجسم وتساعد في الكشف عن المشاكل الصحية ويسبب مجموعة من التحليلات ل",
                DetailsEN = "A set of analyzes to assess the health of the body and help in detecting health pro blems and their causes A set of analyzes to A set of analyzes to assess the health of the body and help in detecting health problems and their causes A set of analyzes to ",
                InstructionAR = "يرجى إبلاغ الطبيب عن نوع الحالة المرضية والتاريخ المرضي السابق للحالة. يقوم فريقنا الطبي والتمريضي باستخدام أجهزة الفحص السريري والمؤشرات الحيوية أثناء الزيارة. تشمل خدماتنا الرعاية المنزلية لكل الأعمار ومختلف الحالات الطبية.",
                InstructionEN = "Please inform the doctor about the type of medical condition and the previous medical history of the condition. Our medical and nursing team uses clinical examination equipment and vital signs during the visit. Our services include home care for all ages and various medical conditions.",
                ImagePath = "packagesImage/Home-Solutions.jpg"

            };
            Package package36 = new Package()
            {
                Price = 2900,
                ServiceId = Natural.ServiceId,
                CategoryId = categoryNatural5.CategoryId,
                CreateDT = DateTime.Now,
                CreateSystemUserId = "1",
                NameEN = "Physiotherapy for Children",
                NameAR = " العلاج الطبيعي للاطفال",
                SubTitleAR = "8 زيارات في الشهر",
                SubTitleEN = "8 visits per month",
                DetailsAR = "مجموعة من التحليلات لتقييم صحةالجسم ويساعد في الكشف عن العيوب الصحيةوأسبابها مجموعة من التحليلات لمجموعة منتحليلات لتقييم صحة الجسم وتساعد في الكشف عن المشاكل الصحية ويسبب مجموعة من التحليلات ل",
                DetailsEN = "A set of analyzes to assess the health of the body and help in detecting health pro blems and their causes A set of analyzes to A set of analyzes to assess the health of the body and help in detecting health problems and their causes A set of analyzes to ",
                InstructionAR = "يرجى إبلاغ الطبيب عن نوع الحالة المرضية والتاريخ المرضي السابق للحالة. يقوم فريقنا الطبي والتمريضي باستخدام أجهزة الفحص السريري والمؤشرات الحيوية أثناء الزيارة. تشمل خدماتنا الرعاية المنزلية لكل الأعمار ومختلف الحالات الطبية.",
                InstructionEN = "Please inform the doctor about the type of medical condition and the previous medical history of the condition. Our medical and nursing team uses clinical examination equipment and vital signs during the visit. Our services include home care for all ages and various medical conditions.",
                ImagePath = "packagesImage/Home-Solutions.jpg"

            };

            _dbContext.Packages.Add(package28);
            _dbContext.Packages.Add(package29);
            _dbContext.Packages.Add(package30);
            _dbContext.Packages.Add(package31);
            _dbContext.Packages.Add(package32);
            _dbContext.Packages.Add(package33);
            _dbContext.Packages.Add(package34);
            _dbContext.Packages.Add(package35);
            _dbContext.Packages.Add(package36);


            Package package37 = new Package()
            {
                Price = 3000,
                ServiceId = Doctor.ServiceId,
                CategoryId = categoryDoctor1.CategoryId,
                CreateDT = DateTime.Now,
                CreateSystemUserId = "1",
                NameEN = "Physiotherapy for Children",
                NameAR = " العلاج الطبيعي للاطفال",
                SubTitleAR = "8 زيارات في الشهر",
                SubTitleEN = "8 visits per month",
                DetailsAR = "مجموعة من التحليلات لتقييم صحةالجسم ويساعد في الكشف عن العيوب الصحيةوأسبابها مجموعة من التحليلات لمجموعة منتحليلات لتقييم صحة الجسم وتساعد في الكشف عن المشاكل الصحية ويسبب مجموعة من التحليلات ل",
                DetailsEN = "A set of analyzes to assess the health of the body and help in detecting health pro blems and their causes A set of analyzes to A set of analyzes to assess the health of the body and help in detecting health problems and their causes A set of analyzes to ",
                InstructionAR = "يرجى إبلاغ الطبيب عن نوع الحالة المرضية والتاريخ المرضي السابق للحالة. يقوم فريقنا الطبي والتمريضي باستخدام أجهزة الفحص السريري والمؤشرات الحيوية أثناء الزيارة. تشمل خدماتنا الرعاية المنزلية لكل الأعمار ومختلف الحالات الطبية.",
                InstructionEN = "Please inform the doctor about the type of medical condition and the previous medical history of the condition. Our medical and nursing team uses clinical examination equipment and vital signs during the visit. Our services include home care for all ages and various medical conditions.",
                ImagePath = "packagesImage/imageshc.jpg"

            };

            Package package38 = new Package()
            {
                Price = 3100,
                ServiceId = Doctor.ServiceId,
                CategoryId = categoryDoctor2.CategoryId,
                CreateDT = DateTime.Now,
                CreateSystemUserId = "1",
                NameEN = "Physiotherapy for Children",
                NameAR = " العلاج الطبيعي للاطفال",
                SubTitleAR = "8 زيارات في الشهر",
                SubTitleEN = "8 visits per month",
                DetailsAR = "مجموعة من التحليلات لتقييم صحةالجسم ويساعد في الكشف عن العيوب الصحيةوأسبابها مجموعة من التحليلات لمجموعة منتحليلات لتقييم صحة الجسم وتساعد في الكشف عن المشاكل الصحية ويسبب مجموعة من التحليلات ل",
                DetailsEN = "A set of analyzes to assess the health of the body and help in detecting health pro blems and their causes A set of analyzes to A set of analyzes to assess the health of the body and help in detecting health problems and their causes A set of analyzes to ",
                InstructionAR = "يرجى إبلاغ الطبيب عن نوع الحالة المرضية والتاريخ المرضي السابق للحالة. يقوم فريقنا الطبي والتمريضي باستخدام أجهزة الفحص السريري والمؤشرات الحيوية أثناء الزيارة. تشمل خدماتنا الرعاية المنزلية لكل الأعمار ومختلف الحالات الطبية.",
                InstructionEN = "Please inform the doctor about the type of medical condition and the previous medical history of the condition. Our medical and nursing team uses clinical examination equipment and vital signs during the visit. Our services include home care for all ages and various medical conditions.",
                ImagePath = "packagesImage/imageshc.jpg"

            };
            Package package39 = new Package()
            {
                Price = 3300,
                ServiceId = Doctor.ServiceId,
                CategoryId = categoryDoctor2.CategoryId,
                CreateDT = DateTime.Now,
                CreateSystemUserId = "1",
                NameEN = "Physiotherapy for Children",
                NameAR = " العلاج الطبيعي للاطفال",
                SubTitleAR = "8 زيارات في الشهر",
                SubTitleEN = "8 visits per month",
                DetailsAR = "مجموعة من التحليلات لتقييم صحةالجسم ويساعد في الكشف عن العيوب الصحيةوأسبابها مجموعة من التحليلات لمجموعة منتحليلات لتقييم صحة الجسم وتساعد في الكشف عن المشاكل الصحية ويسبب مجموعة من التحليلات ل",
                DetailsEN = "A set of analyzes to assess the health of the body and help in detecting health pro blems and their causes A set of analyzes to A set of analyzes to assess the health of the body and help in detecting health problems and their causes A set of analyzes to ",
                InstructionAR = "يرجى إبلاغ الطبيب عن نوع الحالة المرضية والتاريخ المرضي السابق للحالة. يقوم فريقنا الطبي والتمريضي باستخدام أجهزة الفحص السريري والمؤشرات الحيوية أثناء الزيارة. تشمل خدماتنا الرعاية المنزلية لكل الأعمار ومختلف الحالات الطبية.",
                InstructionEN = "Please inform the doctor about the type of medical condition and the previous medical history of the condition. Our medical and nursing team uses clinical examination equipment and vital signs during the visit. Our services include home care for all ages and various medical conditions.",
                ImagePath = "packagesImage/imageshc.jpg"

            };
            Package package40 = new Package()
            {
                Price = 3400,
                ServiceId = Doctor.ServiceId,
                CategoryId = categoryDoctor2.CategoryId,
                CreateDT = DateTime.Now,
                CreateSystemUserId = "1",
                NameEN = "Physiotherapy for Children",
                NameAR = " العلاج الطبيعي للاطفال",
                SubTitleAR = "8 زيارات في الشهر",
                SubTitleEN = "8 visits per month",
                DetailsAR = "مجموعة من التحليلات لتقييم صحةالجسم ويساعد في الكشف عن العيوب الصحيةوأسبابها مجموعة من التحليلات لمجموعة منتحليلات لتقييم صحة الجسم وتساعد في الكشف عن المشاكل الصحية ويسبب مجموعة من التحليلات ل",
                DetailsEN = "A set of analyzes to assess the health of the body and help in detecting health pro blems and their causes A set of analyzes to A set of analyzes to assess the health of the body and help in detecting health problems and their causes A set of analyzes to ",
                InstructionAR = "يرجى إبلاغ الطبيب عن نوع الحالة المرضية والتاريخ المرضي السابق للحالة. يقوم فريقنا الطبي والتمريضي باستخدام أجهزة الفحص السريري والمؤشرات الحيوية أثناء الزيارة. تشمل خدماتنا الرعاية المنزلية لكل الأعمار ومختلف الحالات الطبية.",
                InstructionEN = "Please inform the doctor about the type of medical condition and the previous medical history of the condition. Our medical and nursing team uses clinical examination equipment and vital signs during the visit. Our services include home care for all ages and various medical conditions.",
                ImagePath = "packagesImage/imageshc.jpg"

            };

            Package package41 = new Package()
            {
                Price = 3500,
                ServiceId = Doctor.ServiceId,
                CategoryId = categoryDoctor3.CategoryId,
                CreateDT = DateTime.Now,
                CreateSystemUserId = "1",
                NameEN = "Physiotherapy for Children",
                NameAR = " العلاج الطبيعي للاطفال",
                SubTitleAR = "8 زيارات في الشهر",
                SubTitleEN = "8 visits per month",
                DetailsAR = "مجموعة من التحليلات لتقييم صحةالجسم ويساعد في الكشف عن العيوب الصحيةوأسبابها مجموعة من التحليلات لمجموعة منتحليلات لتقييم صحة الجسم وتساعد في الكشف عن المشاكل الصحية ويسبب مجموعة من التحليلات ل",
                DetailsEN = "A set of analyzes to assess the health of the body and help in detecting health pro blems and their causes A set of analyzes to A set of analyzes to assess the health of the body and help in detecting health problems and their causes A set of analyzes to ",
                InstructionAR = "يرجى إبلاغ الطبيب عن نوع الحالة المرضية والتاريخ المرضي السابق للحالة. يقوم فريقنا الطبي والتمريضي باستخدام أجهزة الفحص السريري والمؤشرات الحيوية أثناء الزيارة. تشمل خدماتنا الرعاية المنزلية لكل الأعمار ومختلف الحالات الطبية.",
                InstructionEN = "Please inform the doctor about the type of medical condition and the previous medical history of the condition. Our medical and nursing team uses clinical examination equipment and vital signs during the visit. Our services include home care for all ages and various medical conditions.",
                ImagePath = "packagesImage/imageshc.jpg"

            };
            Package package42 = new Package()
            {
                Price = 3600,
                ServiceId = Doctor.ServiceId,
                CategoryId = categoryDoctor3.CategoryId,
                CreateDT = DateTime.Now,
                CreateSystemUserId = "1",
                NameEN = "Physiotherapy for Children",
                NameAR = " العلاج الطبيعي للاطفال",
                SubTitleAR = "8 زيارات في الشهر",
                SubTitleEN = "8 visits per month",
                DetailsAR = "مجموعة من التحليلات لتقييم صحةالجسم ويساعد في الكشف عن العيوب الصحيةوأسبابها مجموعة من التحليلات لمجموعة منتحليلات لتقييم صحة الجسم وتساعد في الكشف عن المشاكل الصحية ويسبب مجموعة من التحليلات ل",
                DetailsEN = "A set of analyzes to assess the health of the body and help in detecting health pro blems and their causes A set of analyzes to A set of analyzes to assess the health of the body and help in detecting health problems and their causes A set of analyzes to ",
                InstructionAR = "يرجى إبلاغ الطبيب عن نوع الحالة المرضية والتاريخ المرضي السابق للحالة. يقوم فريقنا الطبي والتمريضي باستخدام أجهزة الفحص السريري والمؤشرات الحيوية أثناء الزيارة. تشمل خدماتنا الرعاية المنزلية لكل الأعمار ومختلف الحالات الطبية.",
                InstructionEN = "Please inform the doctor about the type of medical condition and the previous medical history of the condition. Our medical and nursing team uses clinical examination equipment and vital signs during the visit. Our services include home care for all ages and various medical conditions.",
                ImagePath = "packagesImage/imageshc.jpg"

            };

            Package package43 = new Package()
            {
                Price = 3700,
                ServiceId = Doctor.ServiceId,
                CategoryId = categoryDoctor4.CategoryId,
                CreateDT = DateTime.Now,
                CreateSystemUserId = "1",
                NameEN = "Physiotherapy for Children",
                NameAR = " العلاج الطبيعي للاطفال",
                SubTitleAR = "8 زيارات في الشهر",
                SubTitleEN = "8 visits per month",
                DetailsAR = "مجموعة من التحليلات لتقييم صحةالجسم ويساعد في الكشف عن العيوب الصحيةوأسبابها مجموعة من التحليلات لمجموعة منتحليلات لتقييم صحة الجسم وتساعد في الكشف عن المشاكل الصحية ويسبب مجموعة من التحليلات ل",
                DetailsEN = "A set of analyzes to assess the health of the body and help in detecting health pro blems and their causes A set of analyzes to A set of analyzes to assess the health of the body and help in detecting health problems and their causes A set of analyzes to ",
                InstructionAR = "يرجى إبلاغ الطبيب عن نوع الحالة المرضية والتاريخ المرضي السابق للحالة. يقوم فريقنا الطبي والتمريضي باستخدام أجهزة الفحص السريري والمؤشرات الحيوية أثناء الزيارة. تشمل خدماتنا الرعاية المنزلية لكل الأعمار ومختلف الحالات الطبية.",
                InstructionEN = "Please inform the doctor about the type of medical condition and the previous medical history of the condition. Our medical and nursing team uses clinical examination equipment and vital signs during the visit. Our services include home care for all ages and various medical conditions.",
                ImagePath = "packagesImage/imageshc.jpg"

            };

            Package package44 = new Package()
            {
                Price = 3800,
                ServiceId = Doctor.ServiceId,
                CategoryId = categoryDoctor5.CategoryId,
                CreateDT = DateTime.Now,
                CreateSystemUserId = "1",
                NameEN = "Physiotherapy for Children",
                NameAR = " العلاج الطبيعي للاطفال",
                SubTitleAR = "8 زيارات في الشهر",
                SubTitleEN = "8 visits per month",
                DetailsAR = "مجموعة من التحليلات لتقييم صحةالجسم ويساعد في الكشف عن العيوب الصحيةوأسبابها مجموعة من التحليلات لمجموعة منتحليلات لتقييم صحة الجسم وتساعد في الكشف عن المشاكل الصحية ويسبب مجموعة من التحليلات ل",
                DetailsEN = "A set of analyzes to assess the health of the body and help in detecting health pro blems and their causes A set of analyzes to A set of analyzes to assess the health of the body and help in detecting health problems and their causes A set of analyzes to ",
                InstructionAR = "يرجى إبلاغ الطبيب عن نوع الحالة المرضية والتاريخ المرضي السابق للحالة. يقوم فريقنا الطبي والتمريضي باستخدام أجهزة الفحص السريري والمؤشرات الحيوية أثناء الزيارة. تشمل خدماتنا الرعاية المنزلية لكل الأعمار ومختلف الحالات الطبية.",
                InstructionEN = "Please inform the doctor about the type of medical condition and the previous medical history of the condition. Our medical and nursing team uses clinical examination equipment and vital signs during the visit. Our services include home care for all ages and various medical conditions.",
                ImagePath = "packagesImage/imageshc.jpg"

            };
            Package package45 = new Package()
            {
                Price = 3900,
                ServiceId = Doctor.ServiceId,
                CategoryId = categoryDoctor5.CategoryId,
                CreateDT = DateTime.Now,
                CreateSystemUserId = "1",
                NameEN = "Physiotherapy for Children",
                NameAR = " العلاج الطبيعي للاطفال",
                SubTitleAR = "8 زيارات في الشهر",
                SubTitleEN = "8 visits per month",
                DetailsAR = "مجموعة من التحليلات لتقييم صحةالجسم ويساعد في الكشف عن العيوب الصحيةوأسبابها مجموعة من التحليلات لمجموعة منتحليلات لتقييم صحة الجسم وتساعد في الكشف عن المشاكل الصحية ويسبب مجموعة من التحليلات ل",
                DetailsEN = "A set of analyzes to assess the health of the body and help in detecting health pro blems and their causes A set of analyzes to A set of analyzes to assess the health of the body and help in detecting health problems and their causes A set of analyzes to ",
                InstructionAR = "يرجى إبلاغ الطبيب عن نوع الحالة المرضية والتاريخ المرضي السابق للحالة. يقوم فريقنا الطبي والتمريضي باستخدام أجهزة الفحص السريري والمؤشرات الحيوية أثناء الزيارة. تشمل خدماتنا الرعاية المنزلية لكل الأعمار ومختلف الحالات الطبية.",
                InstructionEN = "Please inform the doctor about the type of medical condition and the previous medical history of the condition. Our medical and nursing team uses clinical examination equipment and vital signs during the visit. Our services include home care for all ages and various medical conditions.",
                ImagePath = "packagesImage/imageshc.jpg"

            };

            _dbContext.Packages.Add(package37);
            _dbContext.Packages.Add(package38);
            _dbContext.Packages.Add(package39);
            _dbContext.Packages.Add(package40);
            _dbContext.Packages.Add(package41);
            _dbContext.Packages.Add(package42);
            _dbContext.Packages.Add(package43);
            _dbContext.Packages.Add(package44);
            _dbContext.Packages.Add(package45);

            

            Package package46 = new Package()
            {
                Price = 4000,
                ServiceId = analyzesa.ServiceId,
                CategoryId = categoryanalyzesa1.CategoryId,
                CreateDT = DateTime.Now,
                CreateSystemUserId = "1",
                NameEN = "Physiotherapy for Children",
                NameAR = " العلاج الطبيعي للاطفال",
                SubTitleAR = "8 زيارات في الشهر",
                SubTitleEN = "8 visits per month",
                DetailsAR = "مجموعة من التحليلات لتقييم صحةالجسم ويساعد في الكشف عن العيوب الصحيةوأسبابها مجموعة من التحليلات لمجموعة منتحليلات لتقييم صحة الجسم وتساعد في الكشف عن المشاكل الصحية ويسبب مجموعة من التحليلات ل",
                DetailsEN = "A set of analyzes to assess the health of the body and help in detecting health pro blems and their causes A set of analyzes to A set of analyzes to assess the health of the body and help in detecting health problems and their causes A set of analyzes to ",
                InstructionAR = "يرجى إبلاغ الطبيب عن نوع الحالة المرضية والتاريخ المرضي السابق للحالة. يقوم فريقنا الطبي والتمريضي باستخدام أجهزة الفحص السريري والمؤشرات الحيوية أثناء الزيارة. تشمل خدماتنا الرعاية المنزلية لكل الأعمار ومختلف الحالات الطبية.",
                InstructionEN = "Please inform the doctor about the type of medical condition and the previous medical history of the condition. Our medical and nursing team uses clinical examination equipment and vital signs during the visit. Our services include home care for all ages and various medical conditions.",
                ImagePath = "packagesImage/Lab.jpg"

            };

            Package package47 = new Package()
            {
                Price = 4100,
                ServiceId = analyzesa.ServiceId,
                CategoryId = categoryanalyzesa2.CategoryId,
                CreateDT = DateTime.Now,
                CreateSystemUserId = "1",
                NameEN = "Physiotherapy for Children",
                NameAR = " العلاج الطبيعي للاطفال",
                SubTitleAR = "8 زيارات في الشهر",
                SubTitleEN = "8 visits per month",
                DetailsAR = "مجموعة من التحليلات لتقييم صحةالجسم ويساعد في الكشف عن العيوب الصحيةوأسبابها مجموعة من التحليلات لمجموعة منتحليلات لتقييم صحة الجسم وتساعد في الكشف عن المشاكل الصحية ويسبب مجموعة من التحليلات ل",
                DetailsEN = "A set of analyzes to assess the health of the body and help in detecting health pro blems and their causes A set of analyzes to A set of analyzes to assess the health of the body and help in detecting health problems and their causes A set of analyzes to ",
                InstructionAR = "يرجى إبلاغ الطبيب عن نوع الحالة المرضية والتاريخ المرضي السابق للحالة. يقوم فريقنا الطبي والتمريضي باستخدام أجهزة الفحص السريري والمؤشرات الحيوية أثناء الزيارة. تشمل خدماتنا الرعاية المنزلية لكل الأعمار ومختلف الحالات الطبية.",
                InstructionEN = "Please inform the doctor about the type of medical condition and the previous medical history of the condition. Our medical and nursing team uses clinical examination equipment and vital signs during the visit. Our services include home care for all ages and various medical conditions.",
                ImagePath = "packagesImage/Lab.jpg"

            };
            Package package48 = new Package()
            {
                Price = 4300,
                ServiceId = analyzesa.ServiceId,
                CategoryId = categoryanalyzesa2.CategoryId,
                CreateDT = DateTime.Now,
                CreateSystemUserId = "1",
                NameEN = "Physiotherapy for Children",
                NameAR = " العلاج الطبيعي للاطفال",
                SubTitleAR = "8 زيارات في الشهر",
                SubTitleEN = "8 visits per month",
                DetailsAR = "مجموعة من التحليلات لتقييم صحةالجسم ويساعد في الكشف عن العيوب الصحيةوأسبابها مجموعة من التحليلات لمجموعة منتحليلات لتقييم صحة الجسم وتساعد في الكشف عن المشاكل الصحية ويسبب مجموعة من التحليلات ل",
                DetailsEN = "A set of analyzes to assess the health of the body and help in detecting health pro blems and their causes A set of analyzes to A set of analyzes to assess the health of the body and help in detecting health problems and their causes A set of analyzes to ",
                InstructionAR = "يرجى إبلاغ الطبيب عن نوع الحالة المرضية والتاريخ المرضي السابق للحالة. يقوم فريقنا الطبي والتمريضي باستخدام أجهزة الفحص السريري والمؤشرات الحيوية أثناء الزيارة. تشمل خدماتنا الرعاية المنزلية لكل الأعمار ومختلف الحالات الطبية.",
                InstructionEN = "Please inform the doctor about the type of medical condition and the previous medical history of the condition. Our medical and nursing team uses clinical examination equipment and vital signs during the visit. Our services include home care for all ages and various medical conditions.",
                ImagePath = "packagesImage/Lab.jpg"

            };
            Package package49 = new Package()
            {
                Price = 4400,
                ServiceId = analyzesa.ServiceId,
                CategoryId = categoryanalyzesa2.CategoryId,
                CreateDT = DateTime.Now,
                CreateSystemUserId = "1",
                NameEN = "Physiotherapy for Children",
                NameAR = " العلاج الطبيعي للاطفال",
                SubTitleAR = "8 زيارات في الشهر",
                SubTitleEN = "8 visits per month",
                DetailsAR = "مجموعة من التحليلات لتقييم صحةالجسم ويساعد في الكشف عن العيوب الصحيةوأسبابها مجموعة من التحليلات لمجموعة منتحليلات لتقييم صحة الجسم وتساعد في الكشف عن المشاكل الصحية ويسبب مجموعة من التحليلات ل",
                DetailsEN = "A set of analyzes to assess the health of the body and help in detecting health pro blems and their causes A set of analyzes to A set of analyzes to assess the health of the body and help in detecting health problems and their causes A set of analyzes to ",
                InstructionAR = "يرجى إبلاغ الطبيب عن نوع الحالة المرضية والتاريخ المرضي السابق للحالة. يقوم فريقنا الطبي والتمريضي باستخدام أجهزة الفحص السريري والمؤشرات الحيوية أثناء الزيارة. تشمل خدماتنا الرعاية المنزلية لكل الأعمار ومختلف الحالات الطبية.",
                InstructionEN = "Please inform the doctor about the type of medical condition and the previous medical history of the condition. Our medical and nursing team uses clinical examination equipment and vital signs during the visit. Our services include home care for all ages and various medical conditions.",
                ImagePath = "packagesImage/Lab.jpg"

            };

            Package package50 = new Package()
            {
                Price = 4500,
                ServiceId = analyzesa.ServiceId,
                CategoryId = categoryanalyzesa3.CategoryId,
                CreateDT = DateTime.Now,
                CreateSystemUserId = "1",
                NameEN = "Physiotherapy for Children",
                NameAR = " العلاج الطبيعي للاطفال",
                SubTitleAR = "8 زيارات في الشهر",
                SubTitleEN = "8 visits per month",
                DetailsAR = "مجموعة من التحليلات لتقييم صحةالجسم ويساعد في الكشف عن العيوب الصحيةوأسبابها مجموعة من التحليلات لمجموعة منتحليلات لتقييم صحة الجسم وتساعد في الكشف عن المشاكل الصحية ويسبب مجموعة من التحليلات ل",
                DetailsEN = "A set of analyzes to assess the health of the body and help in detecting health pro blems and their causes A set of analyzes to A set of analyzes to assess the health of the body and help in detecting health problems and their causes A set of analyzes to ",
                InstructionAR = "يرجى إبلاغ الطبيب عن نوع الحالة المرضية والتاريخ المرضي السابق للحالة. يقوم فريقنا الطبي والتمريضي باستخدام أجهزة الفحص السريري والمؤشرات الحيوية أثناء الزيارة. تشمل خدماتنا الرعاية المنزلية لكل الأعمار ومختلف الحالات الطبية.",
                InstructionEN = "Please inform the doctor about the type of medical condition and the previous medical history of the condition. Our medical and nursing team uses clinical examination equipment and vital signs during the visit. Our services include home care for all ages and various medical conditions.",
                ImagePath = "packagesImage/Lab.jpg"

            };
            Package package51 = new Package()
            {
                Price = 4600,
                ServiceId = analyzesa.ServiceId,
                CategoryId = categoryanalyzesa3.CategoryId,
                CreateDT = DateTime.Now,
                CreateSystemUserId = "1",
                NameEN = "Physiotherapy for Children",
                NameAR = " العلاج الطبيعي للاطفال",
                SubTitleAR = "8 زيارات في الشهر",
                SubTitleEN = "8 visits per month",
                DetailsAR = "مجموعة من التحليلات لتقييم صحةالجسم ويساعد في الكشف عن العيوب الصحيةوأسبابها مجموعة من التحليلات لمجموعة منتحليلات لتقييم صحة الجسم وتساعد في الكشف عن المشاكل الصحية ويسبب مجموعة من التحليلات ل",
                DetailsEN = "A set of analyzes to assess the health of the body and help in detecting health pro blems and their causes A set of analyzes to A set of analyzes to assess the health of the body and help in detecting health problems and their causes A set of analyzes to ",
                InstructionAR = "يرجى إبلاغ الطبيب عن نوع الحالة المرضية والتاريخ المرضي السابق للحالة. يقوم فريقنا الطبي والتمريضي باستخدام أجهزة الفحص السريري والمؤشرات الحيوية أثناء الزيارة. تشمل خدماتنا الرعاية المنزلية لكل الأعمار ومختلف الحالات الطبية.",
                InstructionEN = "Please inform the doctor about the type of medical condition and the previous medical history of the condition. Our medical and nursing team uses clinical examination equipment and vital signs during the visit. Our services include home care for all ages and various medical conditions.",
                ImagePath = "packagesImage/Lab.jpg"

            };

            Package package52 = new Package()
            {
                Price = 4700,
                ServiceId = analyzesa.ServiceId,
                CategoryId = categoryanalyzesa4.CategoryId,
                CreateDT = DateTime.Now,
                CreateSystemUserId = "1",
                NameEN = "Physiotherapy for Children",
                NameAR = " العلاج الطبيعي للاطفال",
                SubTitleAR = "8 زيارات في الشهر",
                SubTitleEN = "8 visits per month",
                DetailsAR = "مجموعة من التحليلات لتقييم صحةالجسم ويساعد في الكشف عن العيوب الصحيةوأسبابها مجموعة من التحليلات لمجموعة منتحليلات لتقييم صحة الجسم وتساعد في الكشف عن المشاكل الصحية ويسبب مجموعة من التحليلات ل",
                DetailsEN = "A set of analyzes to assess the health of the body and help in detecting health pro blems and their causes A set of analyzes to A set of analyzes to assess the health of the body and help in detecting health problems and their causes A set of analyzes to ",
                InstructionAR = "يرجى إبلاغ الطبيب عن نوع الحالة المرضية والتاريخ المرضي السابق للحالة. يقوم فريقنا الطبي والتمريضي باستخدام أجهزة الفحص السريري والمؤشرات الحيوية أثناء الزيارة. تشمل خدماتنا الرعاية المنزلية لكل الأعمار ومختلف الحالات الطبية.",
                InstructionEN = "Please inform the doctor about the type of medical condition and the previous medical history of the condition. Our medical and nursing team uses clinical examination equipment and vital signs during the visit. Our services include home care for all ages and various medical conditions.",
                ImagePath = "packagesImage/Lab.jpg"

            };

            Package package53 = new Package()
            {
                Price = 4800,
                ServiceId = analyzesa.ServiceId,
                CategoryId = categoryanalyzesa5.CategoryId,
                CreateDT = DateTime.Now,
                CreateSystemUserId = "1",
                NameEN = "Physiotherapy for Children",
                NameAR = " العلاج الطبيعي للاطفال",
                SubTitleAR = "8 زيارات في الشهر",
                SubTitleEN = "8 visits per month",
                DetailsAR = "مجموعة من التحليلات لتقييم صحةالجسم ويساعد في الكشف عن العيوب الصحيةوأسبابها مجموعة من التحليلات لمجموعة منتحليلات لتقييم صحة الجسم وتساعد في الكشف عن المشاكل الصحية ويسبب مجموعة من التحليلات ل",
                DetailsEN = "A set of analyzes to assess the health of the body and help in detecting health pro blems and their causes A set of analyzes to A set of analyzes to assess the health of the body and help in detecting health problems and their causes A set of analyzes to ",
                InstructionAR = "يرجى إبلاغ الطبيب عن نوع الحالة المرضية والتاريخ المرضي السابق للحالة. يقوم فريقنا الطبي والتمريضي باستخدام أجهزة الفحص السريري والمؤشرات الحيوية أثناء الزيارة. تشمل خدماتنا الرعاية المنزلية لكل الأعمار ومختلف الحالات الطبية.",
                InstructionEN = "Please inform the doctor about the type of medical condition and the previous medical history of the condition. Our medical and nursing team uses clinical examination equipment and vital signs during the visit. Our services include home care for all ages and various medical conditions.",
                ImagePath = "packagesImage/Lab.jpg"

            };
            Package package54 = new Package()
            {
                Price = 4900,
                ServiceId = analyzesa.ServiceId,
                CategoryId = categoryanalyzesa5.CategoryId,
                CreateDT = DateTime.Now,
                CreateSystemUserId = "1",
                NameEN = "Physiotherapy for Children",
                NameAR = " العلاج الطبيعي للاطفال",
                SubTitleAR = "8 زيارات في الشهر",
                SubTitleEN = "8 visits per month",
                DetailsAR = "مجموعة من التحليلات لتقييم صحةالجسم ويساعد في الكشف عن العيوب الصحيةوأسبابها مجموعة من التحليلات لمجموعة منتحليلات لتقييم صحة الجسم وتساعد في الكشف عن المشاكل الصحية ويسبب مجموعة من التحليلات ل",
                DetailsEN = "A set of analyzes to assess the health of the body and help in detecting health pro blems and their causes A set of analyzes to A set of analyzes to assess the health of the body and help in detecting health problems and their causes A set of analyzes to ",
                InstructionAR = "يرجى إبلاغ الطبيب عن نوع الحالة المرضية والتاريخ المرضي السابق للحالة. يقوم فريقنا الطبي والتمريضي باستخدام أجهزة الفحص السريري والمؤشرات الحيوية أثناء الزيارة. تشمل خدماتنا الرعاية المنزلية لكل الأعمار ومختلف الحالات الطبية.",
                InstructionEN = "Please inform the doctor about the type of medical condition and the previous medical history of the condition. Our medical and nursing team uses clinical examination equipment and vital signs during the visit. Our services include home care for all ages and various medical conditions.",
                ImagePath = "packagesImage/Lab.jpg"

            };

            _dbContext.Packages.Add(package46);
            _dbContext.Packages.Add(package47);
            _dbContext.Packages.Add(package48);
            _dbContext.Packages.Add(package49);
            _dbContext.Packages.Add(package50);
            _dbContext.Packages.Add(package51);
            _dbContext.Packages.Add(package52);
            _dbContext.Packages.Add(package53);
            _dbContext.Packages.Add(package54);


            Package package55 = new Package()
            {
                Price = 5000,
                ServiceId = analyzesb.ServiceId,
                CategoryId = categoryanalyzesb1.CategoryId,
                CreateDT = DateTime.Now,
                CreateSystemUserId = "1",
                NameEN = "Physiotherapy for Children",
                NameAR = " العلاج الطبيعي للاطفال",
                SubTitleAR = "8 زيارات في الشهر",
                SubTitleEN = "8 visits per month",
                DetailsAR = "مجموعة من التحليلات لتقييم صحةالجسم ويساعد في الكشف عن العيوب الصحيةوأسبابها مجموعة من التحليلات لمجموعة منتحليلات لتقييم صحة الجسم وتساعد في الكشف عن المشاكل الصحية ويسبب مجموعة من التحليلات ل",
                DetailsEN = "A set of analyzes to assess the health of the body and help in detecting health pro blems and their causes A set of analyzes to A set of analyzes to assess the health of the body and help in detecting health problems and their causes A set of analyzes to ",
                InstructionAR = "يرجى إبلاغ الطبيب عن نوع الحالة المرضية والتاريخ المرضي السابق للحالة. يقوم فريقنا الطبي والتمريضي باستخدام أجهزة الفحص السريري والمؤشرات الحيوية أثناء الزيارة. تشمل خدماتنا الرعاية المنزلية لكل الأعمار ومختلف الحالات الطبية.",
                InstructionEN = "Please inform the doctor about the type of medical condition and the previous medical history of the condition. Our medical and nursing team uses clinical examination equipment and vital signs during the visit. Our services include home care for all ages and various medical conditions.",
                ImagePath = "packagesImage/preparing_for_lab_test.png"

            };

            Package package56 = new Package()
            {
                Price = 5100,
                ServiceId = analyzesb.ServiceId,
                CategoryId = categoryanalyzesb2.CategoryId,
                CreateDT = DateTime.Now,
                CreateSystemUserId = "1",
                NameEN = "Physiotherapy for Children",
                NameAR = " العلاج الطبيعي للاطفال",
                SubTitleAR = "8 زيارات في الشهر",
                SubTitleEN = "8 visits per month",
                DetailsAR = "مجموعة من التحليلات لتقييم صحةالجسم ويساعد في الكشف عن العيوب الصحيةوأسبابها مجموعة من التحليلات لمجموعة منتحليلات لتقييم صحة الجسم وتساعد في الكشف عن المشاكل الصحية ويسبب مجموعة من التحليلات ل",
                DetailsEN = "A set of analyzes to assess the health of the body and help in detecting health pro blems and their causes A set of analyzes to A set of analyzes to assess the health of the body and help in detecting health problems and their causes A set of analyzes to ",
                InstructionAR = "يرجى إبلاغ الطبيب عن نوع الحالة المرضية والتاريخ المرضي السابق للحالة. يقوم فريقنا الطبي والتمريضي باستخدام أجهزة الفحص السريري والمؤشرات الحيوية أثناء الزيارة. تشمل خدماتنا الرعاية المنزلية لكل الأعمار ومختلف الحالات الطبية.",
                InstructionEN = "Please inform the doctor about the type of medical condition and the previous medical history of the condition. Our medical and nursing team uses clinical examination equipment and vital signs during the visit. Our services include home care for all ages and various medical conditions.",
                ImagePath = "packagesImage/preparing_for_lab_test.png"

            };
            Package package57 = new Package()
            {
                Price = 5300,
                ServiceId = analyzesb.ServiceId,
                CategoryId = categoryanalyzesb2.CategoryId,
                CreateDT = DateTime.Now,
                CreateSystemUserId = "1",
                NameEN = "Physiotherapy for Children",
                NameAR = " العلاج الطبيعي للاطفال",
                SubTitleAR = "8 زيارات في الشهر",
                SubTitleEN = "8 visits per month",
                DetailsAR = "مجموعة من التحليلات لتقييم صحةالجسم ويساعد في الكشف عن العيوب الصحيةوأسبابها مجموعة من التحليلات لمجموعة منتحليلات لتقييم صحة الجسم وتساعد في الكشف عن المشاكل الصحية ويسبب مجموعة من التحليلات ل",
                DetailsEN = "A set of analyzes to assess the health of the body and help in detecting health pro blems and their causes A set of analyzes to A set of analyzes to assess the health of the body and help in detecting health problems and their causes A set of analyzes to ",
                InstructionAR = "يرجى إبلاغ الطبيب عن نوع الحالة المرضية والتاريخ المرضي السابق للحالة. يقوم فريقنا الطبي والتمريضي باستخدام أجهزة الفحص السريري والمؤشرات الحيوية أثناء الزيارة. تشمل خدماتنا الرعاية المنزلية لكل الأعمار ومختلف الحالات الطبية.",
                InstructionEN = "Please inform the doctor about the type of medical condition and the previous medical history of the condition. Our medical and nursing team uses clinical examination equipment and vital signs during the visit. Our services include home care for all ages and various medical conditions.",
                ImagePath = "packagesImage/preparing_for_lab_test.png"

            };
            Package package58 = new Package()
            {
                Price = 5400,
                ServiceId = analyzesb.ServiceId,
                CategoryId = categoryanalyzesb2.CategoryId,
                CreateDT = DateTime.Now,
                CreateSystemUserId = "1",
                NameEN = "Physiotherapy for Children",
                NameAR = " العلاج الطبيعي للاطفال",
                SubTitleAR = "8 زيارات في الشهر",
                SubTitleEN = "8 visits per month",
                DetailsAR = "مجموعة من التحليلات لتقييم صحةالجسم ويساعد في الكشف عن العيوب الصحيةوأسبابها مجموعة من التحليلات لمجموعة منتحليلات لتقييم صحة الجسم وتساعد في الكشف عن المشاكل الصحية ويسبب مجموعة من التحليلات ل",
                DetailsEN = "A set of analyzes to assess the health of the body and help in detecting health pro blems and their causes A set of analyzes to A set of analyzes to assess the health of the body and help in detecting health problems and their causes A set of analyzes to ",
                InstructionAR = "يرجى إبلاغ الطبيب عن نوع الحالة المرضية والتاريخ المرضي السابق للحالة. يقوم فريقنا الطبي والتمريضي باستخدام أجهزة الفحص السريري والمؤشرات الحيوية أثناء الزيارة. تشمل خدماتنا الرعاية المنزلية لكل الأعمار ومختلف الحالات الطبية.",
                InstructionEN = "Please inform the doctor about the type of medical condition and the previous medical history of the condition. Our medical and nursing team uses clinical examination equipment and vital signs during the visit. Our services include home care for all ages and various medical conditions.",
                ImagePath = "packagesImage/preparing_for_lab_test.png"

            };

            Package package59 = new Package()
            {
                Price = 5500,
                ServiceId = analyzesb.ServiceId,
                CategoryId = categoryanalyzesb3.CategoryId,
                CreateDT = DateTime.Now,
                CreateSystemUserId = "1",
                NameEN = "Physiotherapy for Children",
                NameAR = " العلاج الطبيعي للاطفال",
                SubTitleAR = "8 زيارات في الشهر",
                SubTitleEN = "8 visits per month",
                DetailsAR = "مجموعة من التحليلات لتقييم صحةالجسم ويساعد في الكشف عن العيوب الصحيةوأسبابها مجموعة من التحليلات لمجموعة منتحليلات لتقييم صحة الجسم وتساعد في الكشف عن المشاكل الصحية ويسبب مجموعة من التحليلات ل",
                DetailsEN = "A set of analyzes to assess the health of the body and help in detecting health pro blems and their causes A set of analyzes to A set of analyzes to assess the health of the body and help in detecting health problems and their causes A set of analyzes to ",
                InstructionAR = "يرجى إبلاغ الطبيب عن نوع الحالة المرضية والتاريخ المرضي السابق للحالة. يقوم فريقنا الطبي والتمريضي باستخدام أجهزة الفحص السريري والمؤشرات الحيوية أثناء الزيارة. تشمل خدماتنا الرعاية المنزلية لكل الأعمار ومختلف الحالات الطبية.",
                InstructionEN = "Please inform the doctor about the type of medical condition and the previous medical history of the condition. Our medical and nursing team uses clinical examination equipment and vital signs during the visit. Our services include home care for all ages and various medical conditions.",
                ImagePath = "packagesImage/preparing_for_lab_test.png"

            };
            Package package60 = new Package()
            {
                Price = 5600,
                ServiceId = analyzesb.ServiceId,
                CategoryId = categoryanalyzesb3.CategoryId,
                CreateDT = DateTime.Now,
                CreateSystemUserId = "1",
                NameEN = "Physiotherapy for Children",
                NameAR = " العلاج الطبيعي للاطفال",
                SubTitleAR = "8 زيارات في الشهر",
                SubTitleEN = "8 visits per month",
                DetailsAR = "مجموعة من التحليلات لتقييم صحةالجسم ويساعد في الكشف عن العيوب الصحيةوأسبابها مجموعة من التحليلات لمجموعة منتحليلات لتقييم صحة الجسم وتساعد في الكشف عن المشاكل الصحية ويسبب مجموعة من التحليلات ل",
                DetailsEN = "A set of analyzes to assess the health of the body and help in detecting health pro blems and their causes A set of analyzes to A set of analyzes to assess the health of the body and help in detecting health problems and their causes A set of analyzes to ",
                InstructionAR = "يرجى إبلاغ الطبيب عن نوع الحالة المرضية والتاريخ المرضي السابق للحالة. يقوم فريقنا الطبي والتمريضي باستخدام أجهزة الفحص السريري والمؤشرات الحيوية أثناء الزيارة. تشمل خدماتنا الرعاية المنزلية لكل الأعمار ومختلف الحالات الطبية.",
                InstructionEN = "Please inform the doctor about the type of medical condition and the previous medical history of the condition. Our medical and nursing team uses clinical examination equipment and vital signs during the visit. Our services include home care for all ages and various medical conditions.",
                ImagePath = "packagesImage/preparing_for_lab_test.png"

            };

            Package package61 = new Package()
            {
                Price = 5700,
                ServiceId = analyzesb.ServiceId,
                CategoryId = categoryanalyzesb4.CategoryId,
                CreateDT = DateTime.Now,
                CreateSystemUserId = "1",
                NameEN = "Physiotherapy for Children",
                NameAR = " العلاج الطبيعي للاطفال",
                SubTitleAR = "8 زيارات في الشهر",
                SubTitleEN = "8 visits per month",
                DetailsAR = "مجموعة من التحليلات لتقييم صحةالجسم ويساعد في الكشف عن العيوب الصحيةوأسبابها مجموعة من التحليلات لمجموعة منتحليلات لتقييم صحة الجسم وتساعد في الكشف عن المشاكل الصحية ويسبب مجموعة من التحليلات ل",
                DetailsEN = "A set of analyzes to assess the health of the body and help in detecting health pro blems and their causes A set of analyzes to A set of analyzes to assess the health of the body and help in detecting health problems and their causes A set of analyzes to ",
                InstructionAR = "يرجى إبلاغ الطبيب عن نوع الحالة المرضية والتاريخ المرضي السابق للحالة. يقوم فريقنا الطبي والتمريضي باستخدام أجهزة الفحص السريري والمؤشرات الحيوية أثناء الزيارة. تشمل خدماتنا الرعاية المنزلية لكل الأعمار ومختلف الحالات الطبية.",
                InstructionEN = "Please inform the doctor about the type of medical condition and the previous medical history of the condition. Our medical and nursing team uses clinical examination equipment and vital signs during the visit. Our services include home care for all ages and various medical conditions.",
                ImagePath = "packagesImage/preparing_for_lab_test.png"

            };

            Package package62 = new Package()
            {
                Price = 5800,
                ServiceId = analyzesb.ServiceId,
                CategoryId = categoryanalyzesb5.CategoryId,
                CreateDT = DateTime.Now,
                CreateSystemUserId = "1",
                NameEN = "Physiotherapy for Children",
                NameAR = " العلاج الطبيعي للاطفال",
                SubTitleAR = "8 زيارات في الشهر",
                SubTitleEN = "8 visits per month",
                DetailsAR = "مجموعة من التحليلات لتقييم صحةالجسم ويساعد في الكشف عن العيوب الصحيةوأسبابها مجموعة من التحليلات لمجموعة منتحليلات لتقييم صحة الجسم وتساعد في الكشف عن المشاكل الصحية ويسبب مجموعة من التحليلات ل",
                DetailsEN = "A set of analyzes to assess the health of the body and help in detecting health pro blems and their causes A set of analyzes to A set of analyzes to assess the health of the body and help in detecting health problems and their causes A set of analyzes to ",
                InstructionAR = "يرجى إبلاغ الطبيب عن نوع الحالة المرضية والتاريخ المرضي السابق للحالة. يقوم فريقنا الطبي والتمريضي باستخدام أجهزة الفحص السريري والمؤشرات الحيوية أثناء الزيارة. تشمل خدماتنا الرعاية المنزلية لكل الأعمار ومختلف الحالات الطبية.",
                InstructionEN = "Please inform the doctor about the type of medical condition and the previous medical history of the condition. Our medical and nursing team uses clinical examination equipment and vital signs during the visit. Our services include home care for all ages and various medical conditions.",
                ImagePath = "packagesImage/preparing_for_lab_test.png"

            };
            Package package63 = new Package()
            {
                Price = 5900,
                ServiceId = analyzesb.ServiceId,
                CategoryId = categoryanalyzesb5.CategoryId,
                CreateDT = DateTime.Now,
                CreateSystemUserId = "1",
                NameEN = "Physiotherapy for Children",
                NameAR = " العلاج الطبيعي للاطفال",
                SubTitleAR = "8 زيارات في الشهر",
                SubTitleEN = "8 visits per month",
                DetailsAR = "مجموعة من التحليلات لتقييم صحةالجسم ويساعد في الكشف عن العيوب الصحيةوأسبابها مجموعة من التحليلات لمجموعة منتحليلات لتقييم صحة الجسم وتساعد في الكشف عن المشاكل الصحية ويسبب مجموعة من التحليلات ل",
                DetailsEN = "A set of analyzes to assess the health of the body and help in detecting health pro blems and their causes A set of analyzes to A set of analyzes to assess the health of the body and help in detecting health problems and their causes A set of analyzes to ",
                InstructionAR = "يرجى إبلاغ الطبيب عن نوع الحالة المرضية والتاريخ المرضي السابق للحالة. يقوم فريقنا الطبي والتمريضي باستخدام أجهزة الفحص السريري والمؤشرات الحيوية أثناء الزيارة. تشمل خدماتنا الرعاية المنزلية لكل الأعمار ومختلف الحالات الطبية.",
                InstructionEN = "Please inform the doctor about the type of medical condition and the previous medical history of the condition. Our medical and nursing team uses clinical examination equipment and vital signs during the visit. Our services include home care for all ages and various medical conditions.",
                ImagePath = "packagesImage/preparing_for_lab_test.png"

            };

            _dbContext.Packages.Add(package55);
            _dbContext.Packages.Add(package56);
            _dbContext.Packages.Add(package57);
            _dbContext.Packages.Add(package58);
            _dbContext.Packages.Add(package59);
            _dbContext.Packages.Add(package60);
            _dbContext.Packages.Add(package61);
            _dbContext.Packages.Add(package62);
            _dbContext.Packages.Add(package63);
            _dbContext.SaveChanges();
        }
      
    }
}


