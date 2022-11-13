﻿using Calabonga.AspNetCore.AppDefinitions;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Serilog;
using System.Text.Json;

namespace IdentityModule.Web.Definitions.ErrorHandling
{
    /// <summary>
    /// Custom Error handling 
    /// </summary>
    public class ErrorHandlingDefinition : AppDefinition
    {
        /// <summary>
        /// Configure application for current application
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        public override void ConfigureApplication(WebApplication app) =>
            app.UseExceptionHandler(error => error.Run(async context =>
            {

                context.Response.ContentType = "application/json";
                var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                if (contextFeature is not null)
                {
                    // handling validation errors
                    if (contextFeature.Error is ValidationException failures)
                    {
                        await context.Response.WriteAsync(JsonSerializer.Serialize(failures.Errors));
                        context.Response.StatusCode = StatusCodes.Status400BadRequest;
                        return;
                    }

                    // handling all another errors 
                    Log.Error($"Something went wrong in the {contextFeature.Error}");
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;

                    if (app.Environment.IsDevelopment())
                    {
                        await context.Response.WriteAsync($"INTERNAL SERVER ERROR: {contextFeature.Error}");
                    }
                    else
                    {
                        await context.Response.WriteAsync("INTERNAL SERVER ERROR. PLEASE TRY AGAIN LATER");
                    }
                }
            }));
    }
}