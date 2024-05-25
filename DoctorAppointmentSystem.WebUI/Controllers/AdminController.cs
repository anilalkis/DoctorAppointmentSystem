﻿using DoctorAppointmentSystem.Data.Abstract;
using DoctorAppointmentSystem.Entity;
using DoctorAppointmentSystem.WebUI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Numerics;
using System.Security.AccessControl;

namespace DoctorAppointmentSystem.WebUI.Controllers
{
    public class AdminController : Controller
    {
        private IPatientDal _patientDal;
        private IAppointmentDal _appointmentDal;
        private IDoctorDal _doctorDal;
        private IScheduleDal _scheduleDal;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;

        public AdminController(IPatientDal patientDal, IAppointmentDal appointmentDal, IDoctorDal doctorDal, IScheduleDal scheduleDal, UserManager<AppUser> userManager, RoleManager<AppRole> roleManager)
        {
            _patientDal = patientDal;
            _appointmentDal = appointmentDal;
            _doctorDal = doctorDal;
            _scheduleDal = scheduleDal;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public IActionResult Index()
        {
            return View(new AdminDashboardViewModel()
            {
                doctors = _doctorDal.GetAll(),
                patients = _patientDal.GetAll(),
                appointments = _appointmentDal.GetAll(),
            }); 
        }

        public IActionResult DoctorList()
        {
            var doctors = _doctorDal.GetAll();

            return View(doctors);
        }

        [HttpGet]
        public IActionResult UpdateDoctor(int id)
        {
            if(id == 0) 
            {
                return NotFound();
            }

            var doctors = _doctorDal.GetById(id);

            return View(doctors);
        }

        [HttpPost]
        public IActionResult UpdateDoctor(Doctor model)
        {

            if (!ModelState.IsValid)
            {
                Console.WriteLine();
                return NotFound();
            }

            var doctor = _doctorDal.GetByUserEmail(model.Email);

            if (doctor == null)
            {
                return NotFound();
            }

            doctor.FullName = model.FullName;
            doctor.Bio = model.Bio;
            doctor.Email = doctor.Email;
            doctor.Picture = model.Picture;
            doctor.Specialization = model.Specialization;

            _doctorDal.Update(doctor);

            return RedirectToAction("DoctorList");
        }

        [HttpGet]
        public IActionResult CreateDoctor()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateDoctor(Doctor model)
        {
            if (!ModelState.IsValid)
            {
                return NotFound();
            }

            var doctor = new Doctor()
            {
                FullName = model.FullName,
                Bio = model.Bio,
                Email = model.Email,
                Picture = model.Picture,
                Specialization = model.Specialization,
            };

            AppUser user = new AppUser
            {
                FullName = model.FullName,
                UserName = model.Email,
                Email = model.Email,
            };
            
            _doctorDal.Create(doctor);
            await _userManager.CreateAsync(user, "123456");
            await _userManager.AddToRoleAsync(user, "Doctor");
            var doc = _doctorDal.GetByUserEmail(user.Email);
            if(doc == null) 
            {
                return NotFound();
            }

            List<Schedule> schedules = new List<Schedule>()
            {
                new Schedule {DayOfWeek = DayOfWeek.Monday, StartTime = new TimeSpan(9, 0, 0),EndTime = new TimeSpan(17, 0, 0),Interval = new TimeSpan(0, 30, 0), DoctorId = doc.DoctorId,IsWorkingDay = false},
                new Schedule {DayOfWeek = DayOfWeek.Tuesday, StartTime = new TimeSpan(9, 0, 0),EndTime = new TimeSpan(17, 0, 0),Interval = new TimeSpan(0, 30, 0), DoctorId = doc.DoctorId,IsWorkingDay = false},
                new Schedule {DayOfWeek = DayOfWeek.Wednesday, StartTime = new TimeSpan(9, 0, 0),EndTime = new TimeSpan(17, 0, 0),Interval = new TimeSpan(0, 30, 0), DoctorId = doc.DoctorId,IsWorkingDay = false},
                new Schedule {DayOfWeek = DayOfWeek.Thursday, StartTime = new TimeSpan(9, 0, 0),EndTime = new TimeSpan(17, 0, 0),Interval = new TimeSpan(0, 30, 0), DoctorId = doc.DoctorId,IsWorkingDay = false},
                new Schedule {DayOfWeek = DayOfWeek.Friday, StartTime = new TimeSpan(9, 0, 0),EndTime = new TimeSpan(17, 0, 0),Interval = new TimeSpan(0, 30, 0), DoctorId = doc.DoctorId,IsWorkingDay = false},
                new Schedule {DayOfWeek = DayOfWeek.Saturday, StartTime = new TimeSpan(9, 0, 0),EndTime = new TimeSpan(17, 0, 0),Interval = new TimeSpan(0, 30, 0), DoctorId = doc.DoctorId,IsWorkingDay = false},
            };
 
            foreach(var schedule in schedules)
            {
                _scheduleDal.Create(schedule);
            }


            return RedirectToAction("DoctorList");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteDoctor(int id)
        {
            if (id == 0)
            {
                return NotFound();
            }

            var doctor = _doctorDal.GetById(id);
            if (doctor == null)
            {
                return NotFound();
            }
            
            var user = await _userManager.FindByEmailAsync(doctor.Email);
            if (user == null)
            {
                return NotFound();
            }

            _scheduleDal.DeleteSchedulesById(doctor.DoctorId);
            _doctorDal.Delete(doctor);
            await _userManager.DeleteAsync(user);

            return RedirectToAction("DoctorList");
        }

        public IActionResult PatientList() 
        { 
            var patients = _patientDal.GetAll();

            return View(patients); 
        }

        public IActionResult CreatePatient()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreatePatient(Patient model)
        {
            if (ModelState.IsValid)
            {

                var patient = new Patient();

                patient.FullName = model.FullName;
                patient.Gender = model.Gender;
                patient.Email = model.Email;
                patient.Phone = model.Phone;
                patient.Age = model.Age;

                AppUser user = new AppUser
                {
                    FullName = model.FullName,
                    UserName = model.Email,
                    Email = model.Email,
                };

                _patientDal.Create(patient);
                await _userManager.CreateAsync(user, "123456");
                await _userManager.AddToRoleAsync(user, "Patient");
            }

            return RedirectToAction("PatientList");

        }

        [HttpGet]
        public IActionResult UpdatePatient(int id) 
        {
            if (id == 0)
            {
                return NotFound();
            }

            var patient = _patientDal.GetById(id);

            return View(patient);
        }

        [HttpPost]
        public IActionResult UpdatePatient(Patient model)
        {
            if (!ModelState.IsValid)
            {
                return NotFound();
            }

            var patient = _patientDal.GetByUserEmail(model.Email);

            if (patient == null)
            {
                return NotFound();
            }

            patient.FullName = model.FullName;
            patient.Gender = model.Gender;
            patient.Email = patient.Email;
            patient.Age = model.Age;
            patient.Phone = model.Phone;

            _patientDal.Update(patient);

            return RedirectToAction("PatientList");
        }

        [HttpPost]
        public async Task<IActionResult> DeletePatient(int id)
        {
            if (id == 0)
            {
                return NotFound();
            }

            var patient = _patientDal.GetById(id);
            if (patient == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByEmailAsync(patient.Email);
            if (user == null)
            {
                return NotFound();
            }

            _patientDal.Delete(patient);
            await _userManager.DeleteAsync(user);

            return RedirectToAction("PatientList");
        }
    }
}
