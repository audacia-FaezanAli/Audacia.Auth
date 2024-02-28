﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Audacia.Auth.OpenIddict.Authorize;
using Audacia.Auth.OpenIddict.Common;
using Audacia.Auth.OpenIddict.Common.Configuration;
using Audacia.Auth.OpenIddict.Common.Events;
using Audacia.Auth.OpenIddict.Common.Extensions;
using Audacia.Auth.OpenIddict.Token;
using Audacia.Auth.OpenIddict.Token.Custom;
using Audacia.Auth.OpenIddict.UserInfo;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Audacia.Auth.OpenIddict.DependencyInjection;

/// <summary>
/// Extensions to the <see cref="IServiceCollection"/> type.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("member-design", "AV1130:Return interfaces to unchangeable collections.", Justification = "Allows for further extensions")]
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the given <typeparamref name="TProvider"/> to the dependency injection container
    /// as the implementation of <see cref="IAdditionalClaimsProvider{TUser}"/>.
    /// </summary>
    /// <typeparam name="TProvider">The type of <see cref="IAdditionalClaimsProvider{TUser}"/> implementation.</typeparam>
    /// <typeparam name="TUser">The user type.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> instance to which to add the <typeparamref name="TProvider"/>.</param>
    /// <returns>The given <paramref name="services"/>.</returns>
    public static IServiceCollection AddAdditionalClaimsProvider<TProvider, TUser>(this IServiceCollection services)
        where TProvider : class, IAdditionalClaimsProvider<TUser>
        where TUser : class => services.AddTransient<IAdditionalClaimsProvider<TUser>, TProvider>();

    /// <summary>
    /// Adds the given <typeparamref name="TService"/> to the dependency injection container
    /// as the implementation of <see cref="IProfileService{TUser}"/>.
    /// </summary>
    /// <typeparam name="TService">The type of the <see cref="IProfileService{TUser}"/> implementation.</typeparam>
    /// <typeparam name="TUser">The user type.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> instance to which to add the <typeparamref name="TService"/>.</param>
    /// <returns>The given <paramref name="services"/>.</returns>
    public static IServiceCollection AddProfileService<TService, TUser>(this IServiceCollection services)
        where TService : class, IProfileService<TUser>
        where TUser : class => services.AddTransient<IProfileService<TUser>, TService>();

    /// <summary>
    /// Adds the given <typeparamref name="THandler"/> to the dependency injection container
    /// as the implementation of <see cref="IPostAuthenticateHandler{TUser, TId}"/>.
    /// </summary>
    /// <typeparam name="THandler">The type of the <see cref="IPostAuthenticateHandler{TUser, TId}"/> implementation.</typeparam>
    /// <typeparam name="TUser">The type of user.</typeparam>
    /// <typeparam name="TId">The type of the user's primary key.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> instance to which to add the <typeparamref name="THandler"/>.</param>
    /// <returns>The given <paramref name="services"/>.</returns>
    public static IServiceCollection AddPostAuthenticateHandler<THandler, TUser, TId>(this IServiceCollection services)
        where THandler : class, IPostAuthenticateHandler<TUser, TId>
        where TUser : class
        where TId : IEquatable<TId> => services.AddTransient<IPostAuthenticateHandler<TUser, TId>, THandler>();

    /// <summary>
    /// Adds the given <typeparamref name="TValidator"/> to the dependency injection container
    /// as an implementation of <see cref="ICustomGrantTypeValidator{TUser}"/>.
    /// </summary>
    /// <typeparam name="TValidator">Type of the <see cref="ICustomGrantTypeValidator{TUser}"/> implementation.</typeparam>
    /// <typeparam name="TUser">The type of user.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> instance to which to add the <typeparamref name="TValidator"/>.</param>
    /// <returns>The given <paramref name="services"/>.</returns>
    public static IServiceCollection AddCustomGrantTypeValidator<TValidator, TUser>(this IServiceCollection services)
        where TValidator : class, ICustomGrantTypeValidator<TUser>
        where TUser : class => services.AddTransient<ICustomGrantTypeValidator<TUser>, TValidator>();

    /// <summary>
    /// Adds the given <typeparamref name="T"/> to the dependency injection container
    /// as an implementation of <see cref="IEventService"/>.
    /// </summary>
    /// <typeparam name="T">Type of the <see cref="IEventService"/> implementation.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> instance to which to add the <typeparamref name="T"/>.</param>
    /// <returns>The given <paramref name="services"/>.</returns>
    public static IServiceCollection AddEventService<T>(this IServiceCollection services)
        where T : class, IEventService => services.AddTransient<IEventService, T>();

    /// <summary>
    /// Adds the given <typeparamref name="T"/> to the dependency injection container
    /// as an implementation of <see cref="IEventSink"/>.
    /// </summary>
    /// <typeparam name="T">Type of the <see cref="IEventSink"/> implementation.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> instance to which to add the <typeparamref name="T"/>.</param>
    /// <returns>The given <paramref name="services"/>.</returns>
    public static IServiceCollection AddEventSink<T>(this IServiceCollection services)
        where T : class, IEventSink => services.AddTransient<IEventSink, T>();

    /// <summary>
    /// Adds the given <typeparamref name="T"/> to the dependency injection container
    /// as an implementation of <see cref="IEventSerializer"/>.
    /// </summary>
    /// <typeparam name="T">Type of the <see cref="IEventSerializer"/> implementation.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> instance to which to add the <typeparamref name="T"/>.</param>
    /// <returns>The given <paramref name="services"/>.</returns>
    public static IServiceCollection AddEventSerializer<T>(this IServiceCollection services)
        where T : class, IEventSerializer => services.AddTransient<IEventSerializer, T>();

    /// <summary>
    /// Adds OpenIddict services to the given <paramref name="services"/>.
    /// </summary>
    /// <typeparam name="TUser">The user type.</typeparam>
    /// <typeparam name="TId">The type of the user's primary key.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> object to which to add the services.</param>
    /// <param name="optionsBuilder">A delegate containing the additional OpenIddict configuration.</param>
    /// <param name="userIdGetter">A delegate that, when invoked, gets the ID for a given user.</param>
    /// <param name="openIdConnectConfig">An instance of <see cref="OpenIdConnectConfig"/>.</param>
    /// <param name="hostingEnvironment">The current <see cref="IWebHostEnvironment"/>.</param>
    /// <returns>An instance of <see cref="OpenIddictBuilder"/> to which further configuration can be performed.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="openIdConnectConfig"/> is <see langword="null"/>.</exception>
    [SuppressMessage("Maintainability", "ACL1003:Signature contains too many parameters", Justification = "Needs all parameters.")]
    public static OpenIddictBuilder AddOpenIddict<TUser, TId>(
        this IServiceCollection services,
        Action<OpenIddictCoreBuilder> optionsBuilder,
        Func<TUser, TId> userIdGetter,
        OpenIdConnectConfig openIdConnectConfig,
        IWebHostEnvironment hostingEnvironment)
        where TUser : class
        where TId : IEquatable<TId>
    {
        if (openIdConnectConfig == null) throw new ArgumentNullException(nameof(openIdConnectConfig));

        UserWrapper<TUser, TId>.UserIdGetter = userIdGetter;

        return services
            .AddServices<TUser, TId>()
            .AddSingleton(openIdConnectConfig)
            .ConfigureOpenIddict(optionsBuilder, openIdConnectConfig, hostingEnvironment);
    }

    private static IServiceCollection AddServices<TUser, TId>(this IServiceCollection services)
        where TUser : class
        where TId : IEquatable<TId>
    {
        return services
            .AddAdditionalClaimsProvider<DefaultAdditionalClaimsProvider<TUser>, TUser>()
            .AddProfileService<DefaultProfileService<TUser>, TUser>()
            .AddPostAuthenticateHandler<DefaultPostAuthenticateHandler<TUser, TId>, TUser, TId>()
            .AddTransient<IAuthenticateResultHandler<TUser, TId>, DefaultAuthenticateResultHandler<TUser, TId>>()
            .AddTransient<IGetTokenHandler, DefaultGetTokenHandler>()
            .AddTransient<IUserInfoHandler<TUser, TId>, DefaultUserInfoHandler<TUser, TId>>()
            .AddTransient<IClaimsPrincipalProviderFactory, ClaimsPrincipalProviderFactory<TUser, TId>>()
            .AddTransient<ClientCredentialsClaimPrincipalProvider>()
            .AddTransient<PasswordClaimsPrincipalProvider<TUser, TId>>()
            .AddTransient<CodeExchangeClaimsPrincipalProvider<TUser>>()
            .AddTransient<CustomGrantTypeClaimsPrincipalProvider<TUser>>()
            .AddTransient<ISigningCredentialsProvider, DefaultCredentialsProvider>()
            .AddTransient<IEncryptionCredentialsProvider, DefaultCredentialsProvider>()
            .AddTransient<IUtcTimeProvider, UtcTimeProvider>()
            .AddEventService<DefaultEventService>()
            .AddEventSink<DefaultEventSink>()
            .AddEventSerializer<DefaultJsonEventSerializer>();
    }

    private static OpenIddictBuilder ConfigureOpenIddict(
        this IServiceCollection services,
        Action<OpenIddictCoreBuilder> optionsBuilder,
        OpenIdConnectConfig openIdConnectConfig,
        IWebHostEnvironment hostingEnvironment)
    {
        return services
            .AddOpenIddict()
            .AddCore(optionsBuilder)
            .ConfigureOpenIddictServer(openIdConnectConfig, hostingEnvironment)
            .ConfigureOpenIddictValidation(openIdConnectConfig);
    }

    private static OpenIddictBuilder ConfigureOpenIddictServer(
        this OpenIddictBuilder openIddictBuilder,
        OpenIdConnectConfig openIdConnectConfig,
        IWebHostEnvironment hostingEnvironment)
    {
        return openIddictBuilder.AddServer(options =>
        {
            AddEndpoints(options);
            AddFlows(options, openIdConnectConfig);
            AddScopes(openIdConnectConfig, options);
            SetDefaultTokenLifetimes(options);
            ConfigureSigningCredential(options, openIdConnectConfig, hostingEnvironment);
            AddAspNetCore(options, openIdConnectConfig);
        });
    }

    private static void AddEndpoints(OpenIddictServerBuilder options)
    {
        // Enable endpoints, these need to be explicitly enabled
        options.SetAuthorizationEndpointUris("connect/authorize")
            .SetLogoutEndpointUris("account/logout")
            .SetIntrospectionEndpointUris("connect/introspect")
            .SetTokenEndpointUris("connect/token")
            .SetUserinfoEndpointUris("connect/userinfo");
    }

    private static void AddFlows(OpenIddictServerBuilder options, OpenIdConnectConfig openIdConnectConfig)
    {
        if (openIdConnectConfig.AuthorizationCodeClients?.Any() == true)
        {
            options.AllowAuthorizationCodeFlow()
                .RequireProofKeyForCodeExchange();
        }

        if (openIdConnectConfig.ClientCredentialsClients?.Any() == true)
        {
            options.AllowClientCredentialsFlow();
        }

        if (openIdConnectConfig.ResourceOwnerPasswordClients?.Any() == true)
        {
            options.AllowPasswordFlow();
        }

        options.AllowRefreshTokenFlow();

        if (openIdConnectConfig.CustomGrantTypeClients?.Any() == true)
        {
            foreach (var grantType in openIdConnectConfig.CustomGrantTypeClients.Select(client => client.GrantType).Distinct())
            {
                options.AllowCustomFlow(grantType);
            }
        }
    }

    private static void AddScopes(OpenIdConnectConfig openIdConnectConfig, OpenIddictServerBuilder options)
    {
        // Get configured scopes i.e. "email", "profile", "roles", "bookings-api"...
        var configurableScopes = openIdConnectConfig.AllClients
            .SelectMany(c => c.ClientScopes.EmptyIfNull())
            .Concat(new[]
            { // default required scopes
                    Scopes.Profile,
                    Scopes.Email,
                    Scopes.Roles
            })
            .Distinct()
            .ToArray();
        options.RegisterScopes(configurableScopes);
    }

    private static void SetDefaultTokenLifetimes(OpenIddictServerBuilder options)
    {
        // Globally set the lifetime of your tokens, individual token lifetimes per application resource must be done in the AuthorisationController
        options
            .SetAccessTokenLifetime(TimeSpan.FromMinutes(30))
            .SetRefreshTokenLifetime(TimeSpan.FromDays(7));
    }

    private static void AddAspNetCore(OpenIddictServerBuilder builder, OpenIdConnectConfig openIdConnectConfig)
    {
        // Register the ASP.NET Core host and configure the ASP.NET Core-specific options.
        builder
            .Configure(options => options.Issuer = openIdConnectConfig.Url)
            .UseAspNetCore()
            .EnableStatusCodePagesIntegration()
            .EnableAuthorizationEndpointPassthrough()
            .EnableLogoutEndpointPassthrough()
            .EnableTokenEndpointPassthrough()
            .EnableUserinfoEndpointPassthrough();
    }

    private static OpenIddictBuilder ConfigureOpenIddictValidation(this OpenIddictBuilder openIddictBuilder, OpenIdConnectConfig openIdConnectConfig)
    {
        return openIddictBuilder.AddValidation(options =>
        {
            // Configure the audience accepted by this resource server.
            // The value MUST match the audience associated with the resource scope.
            var audiences = openIdConnectConfig!.AllClients!.Select(x => x.ClientId).ToArray();
            options.AddAudiences(audiences);
            options.UseLocalServer();
            options.UseDataProtection();
            options.UseAspNetCore();
        });
    }

    private static OpenIddictServerBuilder ConfigureSigningCredential(
        OpenIddictServerBuilder openIddictServerBuilder,
        OpenIdConnectConfig openIdConnectConfig,
        IWebHostEnvironment hostingEnvironment)
    {
        if (hostingEnvironment.IsDevelopment())
        {
            return openIddictServerBuilder
                .AddDevelopmentEncryptionCertificate()
                .AddDevelopmentSigningCertificate()
                // Allow the access token to be read by the developer during local development
                // this will cause unexpected behaviour in prod if we rely on manual token inspection
                .DisableAccessTokenEncryption();
        }

        if (string.Equals(openIdConnectConfig.CertificateStoreLocation, CertificateLocationOptions.Custom, StringComparison.OrdinalIgnoreCase))
        {
            // If custom location then the client is responsible for loading the certificates
            return openIddictServerBuilder;
        }

        // Find certificates and validate their existance.
        var (encryptionCertificate, signingCertificate) = GetCertificates(openIddictServerBuilder, openIdConnectConfig);

        return openIddictServerBuilder
            .AddEncryptionCertificate(encryptionCertificate)
            .AddSigningCertificate(signingCertificate);
    }

    private static (X509Certificate2 EncryptionCertificate, X509Certificate2 SigningCertificate) GetCertificates(OpenIddictServerBuilder openIddictServerBuilder, OpenIdConnectConfig openIdConnectConfig)
    {
        if (openIdConnectConfig.EncryptionCertificateThumbprint == openIdConnectConfig.SigningCertificateThumbprint)
        {
            throw new OpenIddictConfigurationException("The certificates used for token encryption and token signing should not be the same.");
        }

        var encryptionCertificate = FindCertificate(openIdConnectConfig.EncryptionCertificateThumbprint, openIdConnectConfig.CertificateStoreLocation);
        if (encryptionCertificate == null)
        {
            throw new OpenIddictConfigurationException("Unable to load token encryption certificate");
        }

        var signingCertificate = FindCertificate(openIdConnectConfig.SigningCertificateThumbprint, openIdConnectConfig.CertificateStoreLocation);
        if (signingCertificate == null)
        {
            throw new OpenIddictConfigurationException("Unable to load token signing certificate");
        }

        return (encryptionCertificate, signingCertificate);
    }

    private static X509Certificate2? FindCertificate(string? certificateThumbprint, string? certificateStoreLocation)
    {
        if (certificateThumbprint == null)
        {
            return null;
        }

        X509Certificate2? certificate = null;
        using (var certificateStore = new X509Store(StoreName.My, certificateStoreLocation.ParseStoreLocation()))
        {
            certificateStore.Open(OpenFlags.ReadOnly);
            var certificateCollection = certificateStore.Certificates.Find(
                X509FindType.FindByThumbprint,
                certificateThumbprint,
                false);
            if (certificateCollection.Count > 0)
            {
                certificate = certificateCollection[0];
            }
        }

        return certificate;
    }
}
