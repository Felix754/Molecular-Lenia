using MolecularLenia.Hubs;
using MolecularLenia.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR().AddMessagePackProtocol();
// Program.cs
builder.Services.AddSingleton<ISimulationSettingsManager, SimulationSettingsManager>();
builder.Services.AddSingleton<IParticleFactory, ParticleFactory>();
builder.Services.AddSingleton<IPhysicsEngine, PhysicsEngine>();
builder.Services.AddSingleton<IBiologyEngine, BiologyEngine>();

builder.Services.AddSingleton<SimulationService>(); // Залишаємо оркестратор
builder.Services.AddHostedService<SimulationWorker>();


var app = builder.Build();

app.UseDefaultFiles(); // Дозволяє серверу автоматично відкривати index.html
app.UseStaticFiles();
app.UseRouting();


app.MapHub<SimulationHub>("/simHub");

app.Run();