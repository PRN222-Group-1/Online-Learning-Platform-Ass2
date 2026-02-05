global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Threading.Tasks;
global using System.ComponentModel.DataAnnotations;
global using System.Net.Http;
global using System.Net.Http.Json;
global using System.Text;
global using System.Text.Json;

global using Microsoft.AspNetCore.Builder;
global using Microsoft.AspNetCore.Hosting;
global using Microsoft.AspNetCore.Http;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.AspNetCore.Mvc.RazorPages;
global using Microsoft.AspNetCore.Authorization;
global using Microsoft.AspNetCore.Authentication;
global using Microsoft.AspNetCore.Authentication.Cookies;
global using Microsoft.AspNetCore.Routing;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Hosting;
global using Microsoft.Extensions.Logging;

global using System.Security.Claims;

global using OnlineLearningPlatformAss2.Service.Services.Interfaces;
global using OnlineLearningPlatformAss2.Service.DTOs.User;
global using OnlineLearningPlatformAss2.Service.DTOs.Course;
global using OnlineLearningPlatformAss2.Service.DTOs.LearningPath;
global using OnlineLearningPlatformAss2.Service.DTOs.Category;
