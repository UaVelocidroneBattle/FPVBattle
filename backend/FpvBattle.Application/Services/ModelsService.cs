using FpvBattle.Application.API;
using FpvBattle.Application.API.Velocidrone;
using FpvBattle.Application.Persistence;
using FpvBattle.Domain;
using Microsoft.EntityFrameworkCore;

namespace FpvBattle.Application.Services;

public class ModelsService
{
    private readonly VelocidroneClient _velocidrone;
    private readonly IApplicationDbContext _db;

    public ModelsService(VelocidroneClient velocidrone, IApplicationDbContext db)
    {
        _velocidrone = velocidrone;
        _db = db;
    }

    public async Task UpdateModelsAsync()
    {
        var dtos = await _velocidrone.ModelsAsync();

        var existingModels = _db.QuadModels
            .ToDictionary(m => m.Id);

        var newModels = new List<QuadModel>();

        foreach (var dto in dtos)
        {
            if (!existingModels.ContainsKey(dto.model_id))
            {
                newModels.Add(new QuadModel
                {
                    Id = dto.model_id,
                    Name = dto.model_name,
                    Class = dto.quad_class
                });
            }
        }

        if (newModels.Count > 0)
        {
            _db.QuadModels.AddRange(newModels);
            await _db.SaveChangesAsync();
        }
    }

    public async Task<int?> GetQuadClassAsync(string quadName)
    {
        var quad = await _db.QuadModels.FirstOrDefaultAsync(x => x.Name == quadName);
        return quad?.Class;
    }

    public async Task<bool> QuadsTheSameClassAsync(string firstQuad, string secondQuad)
    {
        var firstClass = await GetQuadClassAsync(firstQuad);
        var secondClass = await GetQuadClassAsync(secondQuad);

        return firstClass is not null && firstClass == secondClass;
    }
}
