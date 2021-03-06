﻿using System;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Owin;
using Microsoft.Owin.Logging;
using Microsoft.Owin.Security.Authorization;
using Microsoft.Owin.Security.Authorization.Infrastructure;
using Owin;
using WebApi_Custom_Handler;
using WebApi_Custom_Handler.Models;

[assembly: OwinStartup(typeof(Startup))]

namespace WebApi_Custom_Handler
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseErrorPage();
            app.Use(AddEmployeeClaimBeforeAuthorizationCheck);
            
            var config = new HttpConfiguration();
            WebApiConfig.Register(config);
            
            app.UseAuthorization(options =>
            {
                options.AddPolicy(ExampleConstants.EmployeeNumber2Policy, policyBuilder =>
                {
                    policyBuilder.AddRequirements(new EmployeeNumber2Requirement());
                });

                var policyProvider = new DefaultAuthorizationPolicyProvider(options);
                var handlers = new IAuthorizationHandler[] {new EmployeeNumber2Handler()};
                var logger = app.CreateLogger("my logger");
                options.Dependencies.Service = new DefaultAuthorizationService(policyProvider, handlers, logger);
            });

            app.UseWebApi(config);
        }

        private static async Task AddEmployeeClaimBeforeAuthorizationCheck(IOwinContext owinContext, Func<Task> next)
        {
            var currentIdentity = (ClaimsIdentity) owinContext.Authentication.User.Identity;
            if (!currentIdentity.HasClaim(x => x.Type == ExampleConstants.EmployeeClaimType))
            {
                const string currentEmployeeNumber = "2";
                currentIdentity.AddClaim(new Claim(ExampleConstants.EmployeeClaimType, currentEmployeeNumber));
            }

            await next();
        }
    }
}