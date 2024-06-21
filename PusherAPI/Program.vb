Imports Microsoft.AspNetCore.Builder
Imports Microsoft.AspNetCore.Hosting
Imports Microsoft.AspNetCore.Http
Imports Microsoft.Extensions.DependencyInjection
Imports Sentry
Imports Sentry.AspNetCore
Imports Sentry.Profiling

Public Module Program

    Private Sub ConfigureSentry(options As SentryAspNetCoreOptions)
        With options
            .Dsn = "https://e0a63dab8d48bf980091644da3425f5c@o4507391117492224.ingest.us.sentry.io/4507414003056640"
            .Debug = True
            .TracesSampleRate = 1.0
            .ProfilesSampleRate = 1.0
            .AddIntegration(New ProfilingIntegration(TimeSpan.FromMilliseconds(500)))
        End With
    End Sub

    Public Sub Main(args As String())
        Dim builder As WebApplicationBuilder = WebApplication.CreateBuilder(args)
        NotificationService.Key = builder.Configuration.Item("Key")
        builder.Services.AddControllers()
        builder.Services.AddEndpointsApiExplorer()
        builder.Services.AddSwaggerGen()
        builder.Services.AddSingleton(Of NotificationService)
        builder.WebHost.UseSentry(AddressOf ConfigureSentry)

        Dim app As WebApplication = builder.Build()
        app.UseSwagger()
        app.UseSwaggerUI()
        app.UseAuthorization()
        app.MapControllers()
        app.UseWebSockets()
        app.Map("/ws", AddressOf NotificationService.SocketHandler)
#If DEBUG Then
        app.Run("http://localhost:7166")
#Else
        app.Run()
#End If
    End Sub
End Module