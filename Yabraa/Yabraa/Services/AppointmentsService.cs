using Microsoft.AspNetCore.Identity;
using YabraaEF.Models;
using YabraaEF;
using Microsoft.EntityFrameworkCore;
using Yabraa.DTOs;
using YabraaEF.Const;
using System.Net.Http.Headers;

namespace Yabraa.Services
{
    public class AppointmentsService
    {
        private ApplicationDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        public AppointmentsService(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _configuration = configuration;
        }
        public async Task<List<AppointmentDto>> GetAppointmentsByUserId(string Id)
        {
            return await _dbContext.VisitDetails.Where(c => c.ApplicationUserId == Id && c.InvoiceDetails.Invoice.Paid)
               .Include(c => c.ApplicationUser).Include(c => c.VisitStatus).Include(c => c.UserFamily).Include(c => c.Package.Service).Include(c => c.InvoiceDetails.Invoice)
               .Select(c => new AppointmentDto
               {
                   PackageNameAR = c.Package.NameAR,
                   PackageNameEN = c.Package.NameEN,
                   ServiceAR = c.Package.Service.NameAR,
                   ServiceEN = c.Package.Service.NameEN,
                   Price = c.InvoiceDetails.Price,
                   UserFamilyName = c.UserFamily.Name,
                   VisitDT = c.VisitDT,
                   Status = c.VisitStatus.Name,
                   VisitTime = c.VisitDT.TimeOfDay,
                   AppointmentId = c.VisitDetailsId
               }).ToListAsync();

        }
        public async Task<AppointmentDetailsDto> GetAppointmentdByUserIdAndVisitDetailsId(string Id,long VisitDetailsId)
        {
            //var StatusId = _dbContext.VisitStatuses.FirstOrDefault(c=> !c.Deleted && c.Name.Contains("Pending"))?.VisitStatusId;
            string url = _configuration.GetValue<string>("AdminUrl");
            if (string.IsNullOrEmpty(url))
            {
                return null;
            }

            var VisitDetails = _dbContext.VisitDetails.Where(c => c.ApplicationUserId == Id && c.VisitDetailsId == VisitDetailsId && c.InvoiceDetails.Invoice.Paid /*&& c.VisitStatusId == StatusId*/)
               .Include(c => c.VisitAttachments).Include(c => c.VisitNotes).Include(c => c.ApplicationUser).Include(c => c.VisitStatus).Include(c => c.UserFamily).Include(c => c.Package.Service).Include(c => c.InvoiceDetails.Invoice).FirstOrDefault();
            if (VisitDetails is not null)
            {
                return new AppointmentDetailsDto
                {
                    PackageNameAR = VisitDetails.Package.NameAR,
                    PackageNameEN = VisitDetails.Package.NameEN,
                    ServiceAR = VisitDetails.Package.Service.NameAR,
                    ServiceEN = VisitDetails.Package.Service.NameEN,
                    Price = VisitDetails.InvoiceDetails.Price,
                    UserFamilyName = VisitDetails.UserFamily.Name,
                    VisitDT = VisitDetails.VisitDT,
                    Status = VisitDetails.VisitStatus.Name,
                    VisitTime = VisitDetails.VisitDT.TimeOfDay,
                    AppointmentId = VisitDetails.VisitDetailsId,
                    LocationAltitude = VisitDetails.LocationAltitude,
                    LocationLongitude = VisitDetails.LocationLongitude,
                    LocationLatitude = VisitDetails.LocationLatitude,
                    Notes = VisitDetails.Notes,
                    VisitAttachments = VisitDetails?.VisitAttachments.Select(c=> new VisitAttachmentDTO { CreateDTs =c.CreateDTs,Path=$"{url}/{c.Path}", Title=c.Title}).ToList(),
                    VisitNotes= VisitDetails?.VisitNotes.Select(x => new VisitNoteDTO { CreateDTs = x.CreateDTs, Description = x.Description, Title = x.Title }).ToList(),
                };
            }
            return null;

        }
        internal int AppointmentEditValidatione(AppointmentEditDto appointmentEditDto)
        {
            var StatusId = _dbContext.VisitStatuses.FirstOrDefault(c => !c.Deleted && c.Name.Contains("Pending"))?.VisitStatusId;
            if (StatusId.HasValue)
            {
              var VisitDetails =  _dbContext.VisitDetails.Where(c => c.VisitDetailsId == appointmentEditDto.AppointmentId).FirstOrDefault();
                if (VisitDetails is not null)
                {
                    if (VisitDetails.VisitStatusId != StatusId)
                    {
                        return -2;
                    }
                    //DateTime now = General.GetKSATimeZoneNow();
                    //if (now <= VisitDetails.VisitDT)
                    //{
                    //    return -3;
                    //}
                    return 1;
                }

            }
            return -1;
        }
        public async Task<AppointmentDetailsDto> EditAppointment(string Id, AppointmentEditDto appointmentEditDto)
        {
            var VisitDetails = _dbContext.VisitDetails.Where(c => c.ApplicationUserId == Id && c.VisitDetailsId == appointmentEditDto.AppointmentId).Include(c => c.InvoiceDetails.Invoice).FirstOrDefault();
            if (VisitDetails is not null)
            {
                if (VisitDetails.Notes?.Trim() != appointmentEditDto.Notes?.Trim())
                {
                    VisitDetails.Notes = appointmentEditDto.Notes;
                }
                if (VisitDetails.LocationLongitude != appointmentEditDto.LocationLongitude || VisitDetails.LocationLatitude != appointmentEditDto.LocationLatitude || VisitDetails.LocationAltitude != appointmentEditDto.LocationAltitude)
                {
                    var InvoiceDetails = _dbContext.InvoiceDetails.Where(c => c.InvoiceId == VisitDetails.InvoiceDetails.Invoice.InvoiceId).Include(c => c.VisitDetails).ToList();

                    foreach (var item in InvoiceDetails)
                    {
                        foreach (var item2 in item.VisitDetails)
                        {
                            item2.LocationLatitude = appointmentEditDto.LocationLatitude;
                            item2.LocationLongitude = appointmentEditDto.LocationLongitude;
                            item2.LocationAltitude = appointmentEditDto.LocationAltitude;
                        }

                    }

                    VisitDetails.InvoiceDetails.Invoice.LocationLatitude = appointmentEditDto.LocationLatitude;
                    VisitDetails.InvoiceDetails.Invoice.LocationLongitude = appointmentEditDto.LocationLongitude;
                    VisitDetails.InvoiceDetails.Invoice.LocationAltitude = appointmentEditDto.LocationAltitude;

                }
                _dbContext.SaveChanges();
                return await GetAppointmentdByUserIdAndVisitDetailsId(Id, appointmentEditDto.AppointmentId);
            }
            return null;
        }

      
    }
}
