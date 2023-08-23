using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using SmartAdmin.WebUI.Static;
using SmartAdmin.WebUI.ViewModel;
using YabraaEF;
using YabraaEF.Const;
using YabraaEF.Models;
using static SmartAdmin.WebUI.ViewModel.Permissions;

namespace SmartAdmin.WebUI.Services
{
    public class VisitsService
    {
        public ApplicationDbContext _context { get; set; }
        public VisitsService(ApplicationDbContext context)
        {
            _context = context;           
        }
        public async Task<List<VisitIndexViewModel>> GetVisits(string Type, string getBy = "daily",int month = 0)
        {
      
            DateTime saudiDateTime = General.GetKSATimeZoneNow();

            ServiceType serviceType = _context.ServiceTypes.FirstOrDefault(c => c.Enable && c.Name == Type);
            if (serviceType != null)
                return await _context.VisitDetails.Where(c => (c.ServiceTypeId == serviceType.ServiceTypeId ) 
                &&( getBy == "daily" ? c.VisitDT.Date == saudiDateTime.Date : (month > 0 ?  c.VisitDT.Month == month : c.VisitDT.Month == saudiDateTime.Month && c.VisitDT.Year == saudiDateTime.Year)))
                    .Include(c => c.ApplicationUser).Include(c=>c.ServiceType).Include(c => c.VisitStatus).Include(c => c.UserFamily).Include(c => c.Package).Include(c => c.InvoiceDetails.Invoice)
                    .Select(c => new VisitIndexViewModel {
                        PackageId = c.PackageId,
                        ApplicationUserId = c.ApplicationUser.Id,
                        ApplicationUserName = c.ApplicationUser.UserName,
                        PackageName = c.Package.NameAR,
                        payment = c.InvoiceDetails.Invoice.Paid,
                        Price = c.InvoiceDetails.Price,
                        UserFamilyName = c.UserFamily.Name,
                        VisitDetailsId = c.VisitDetailsId,
                        VisitDT = c.VisitDT,
                        Status = c.VisitStatus.Name,
                        CurrentDT = saudiDateTime,
                        Type= c.ServiceType.Name
                        }).ToListAsync();
            return null;
        }
        public async Task<int> GetVisitsCount(string ServiceType, string getBy = "daily", int month = 0)
        {
            ServiceType serviceType = await _context.ServiceTypes.FirstOrDefaultAsync(c => c.Enable && c.Name == ServiceType);

            DateTime saudiDateTime = General.GetKSATimeZoneNow();
            return   _context.VisitDetails.Where(c =>  (c.ServiceTypeId == serviceType.ServiceTypeId ) && (getBy == "daily" ? c.VisitDT.Date == saudiDateTime.Date : (month > 0 ? c.VisitDT.Month == month : c.VisitDT.Month == saudiDateTime.Month && c.VisitDT.Year == saudiDateTime.Year)))
                .Count();
        }

        public async Task<List<VisitIndexViewModel>> GetVisitsBYUserId(string Id)
        {

            DateTime saudiDateTime = General.GetKSATimeZoneNow();

            return await _context.VisitDetails.Where(c => c.ApplicationUserId == Id)
                .Include(c => c.ApplicationUser).Include(c => c.VisitStatus).Include(c => c.UserFamily).Include(c => c.Package).Include(c => c.InvoiceDetails.Invoice)
                .Select(c => new VisitIndexViewModel
                {
                    PackageId = c.PackageId,
                    ApplicationUserId = c.ApplicationUser.Id,
                    ApplicationUserName = c.ApplicationUser.UserName,
                    PackageName = c.Package.NameAR,
                    payment = c.InvoiceDetails.Invoice.Paid,
                    Price = c.InvoiceDetails.Price,
                    UserFamilyName = c.UserFamily.Name,
                    VisitDetailsId = c.VisitDetailsId,
                    VisitDT = c.VisitDT,
                    Status = c.VisitStatus.Name,
                    CurrentDT = saudiDateTime
                }).ToListAsync();
        }

        internal async Task<VisitViewModel> GetVisitById(int visitDetailsId)
        {
           var Visit = await  _context.VisitDetails.Include(c => c.ServiceType).Include(c => c.VisitAttachments).Include(c => c.VisitNotes).Include(c => c.ApplicationUser).Include(c => c.VisitStatus).Include(c => c.UserFamily).Include(c => c.Package).Include(c => c.InvoiceDetails.Invoice).FirstOrDefaultAsync(c=>c.VisitDetailsId==visitDetailsId);
            if (Visit is not null)
            {
                VisitViewModel model = new VisitViewModel
                {
                    PackageId = Visit.PackageId,
                    ApplicationUserId = Visit.ApplicationUser.Id,
                    ApplicationUserName = Visit.ApplicationUser.UserName,
                    PackageNameAR = Visit.Package.NameAR,
                    payment = Visit.InvoiceDetails.Invoice.Paid,
                    Price = Visit.InvoiceDetails.Price,
                    UserFamilyName = Visit.UserFamily.Name,
                    VisitDetailsId = Visit.VisitDetailsId,
                    VisitDT = Visit.VisitDT,
                    Status = Visit.VisitStatus.Name,
                    LocationAltitude = Visit.LocationAltitude,
                    LocationLongitude = Visit.LocationLongitude.Value,
                    LocationLatitude = Visit.LocationLatitude.Value,
                    Notes = Visit.Notes,
                    PackageNameEN=Visit.Package.NameEN,
                    UserFamilyBirthDate = Visit.UserFamily.BirthDate,
                    UserFamilyGender = Visit.UserFamily.Gender == 0 ? "Male":"Female"  ,
                    visitAttachments = Visit.VisitAttachments?.OrderByDescending(c=>c.CreateDTs).ToList(),
                    visitNotes = Visit.VisitNotes?.OrderByDescending(c => c.CreateDTs).ToList(),
                    ServiceType = Visit.ServiceType.Name
                };
                return  model;
            }
            return null;
        }

        internal async Task<long> ChangeStatus(int visitDetailsId, string statusName)
        {
            var Visit = await _context.VisitDetails.FirstOrDefaultAsync(c => c.VisitDetailsId == visitDetailsId);
            var Status = await _context.VisitStatuses.FirstOrDefaultAsync(c => c.Name.Contains(statusName));
            if (Visit is not null && Status is not null)
            {
                Visit.VisitStatusId = Status.VisitStatusId;
                _context.SaveChanges();
                return Visit.VisitDetailsId;
            }
            return -1;
        }


        public async Task<List<VisitIndexViewModel>> Search(string ServiceType, string Status, DateTime From, DateTime? To)
        {
            ServiceType serviceType = await _context.ServiceTypes.FirstOrDefaultAsync(c => c.Enable && c.Name == ServiceType);
            
            DateTime saudiDateTime = General.GetKSATimeZoneNow();
            
            var visitStatus = _context.VisitStatuses.FirstOrDefault(c=>c.Name.Contains(Status));
            //if (visitStatus is  null)
            //{
            //    return null;
            //}
           
            return await _context.VisitDetails.Where(c => (c.ServiceTypeId == serviceType.ServiceTypeId)&&(Status.Contains("All") ?  true : c.VisitStatusId == visitStatus.VisitStatusId )&& (To.HasValue ? (c.VisitDT.Date >= From && c.VisitDT.Date <= To.Value.Date) : c.VisitDT.Date == From))
                .Include(c => c.ApplicationUser).Include(c => c.ServiceType).Include(c => c.VisitStatus).Include(c => c.UserFamily).Include(c => c.Package).Include(c => c.InvoiceDetails.Invoice)
                .Select(c => new VisitIndexViewModel
                {
                    PackageId = c.PackageId,
                    ApplicationUserId = c.ApplicationUser.Id,
                    ApplicationUserName = c.ApplicationUser.UserName,
                    PackageName = c.Package.NameAR,
                    payment = c.InvoiceDetails.Invoice.Paid,
                    Price = c.InvoiceDetails.Price,
                    UserFamilyName = c.UserFamily.Name,
                    VisitDetailsId = c.VisitDetailsId,
                    VisitDT = c.VisitDT,
                    Status = c.VisitStatus.Name,
                    Type = c.ServiceType.Name
                    
                }).ToListAsync();
        }
        internal async Task<long> AddNote(int VisitDetailsId, string Title, string Description, string UserId)
        {
            var Visit = await _context.VisitDetails.FirstOrDefaultAsync(c => c.VisitDetailsId == VisitDetailsId);
            if (Visit is not null && VisitDetailsId >0 && !string.IsNullOrWhiteSpace(Title) && !string.IsNullOrWhiteSpace(Description) && !string.IsNullOrWhiteSpace(UserId))
            {
                VisitNotes visitNote = new VisitNotes()
                {
                    CreateDTs = General.GetKSATimeZoneNow(),
                    CreateSystemUserId = UserId,
                    Description = Description?.Trim(),
                    Title = Title?.Trim(),
                     VisitDetailsId = VisitDetailsId
                };
                _context.VisitNotes.Add(visitNote);
                _context.SaveChanges();
                return visitNote.VisitNoteId;
            }
            return -1;
        }
        internal async Task<long> AddAttachment(int VisitDetailsId, string Title, IFormFile File, string UserId)
        {
            var Visit = await _context.VisitDetails.FirstOrDefaultAsync(c => c.VisitDetailsId == VisitDetailsId);
            if (Visit is not null && VisitDetailsId > 0 && !string.IsNullOrWhiteSpace(Title) && File != null && File.Length > 0 && !string.IsNullOrWhiteSpace(UserId))
            {
                string fileName = await Photo.UploadAsync(File, "visitAttachment");
                if (!string.IsNullOrWhiteSpace(fileName))
                {
                    VisitAttachments attachment = new VisitAttachments()
                    {
                        CreateDTs = General.GetKSATimeZoneNow(),
                        CreateSystemUserId = UserId,
                        Path = fileName,
                        Title = Title?.Trim(),
                        VisitDetailsId = VisitDetailsId
                    };
                    _context.VisitAttachments.Add(attachment);
                    _context.SaveChanges();
                    return attachment.VisitAttachmentId;
                }
                
            }
            return -1;
        }

    }
}
