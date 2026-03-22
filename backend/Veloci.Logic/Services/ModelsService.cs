using Microsoft.EntityFrameworkCore;
using Veloci.Data.Domain;
using Veloci.Data.Repositories;
using Veloci.Logic.API;

namespace Veloci.Logic.Services;

public class ModelsService
{
    private readonly Velocidrone _velocidrone;
    private readonly IRepository<QuadModel> _quadModels;

    public ModelsService(Velocidrone velocidrone, IRepository<QuadModel> quadModels)
    {
        _velocidrone = velocidrone;
        _quadModels = quadModels;
    }

    public async Task UpdateModelsAsync()
    {
        var dtos = await _velocidrone.ModelsAsync();

        var existingModels = _quadModels.GetAll()
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
            await _quadModels.AddRangeAsync(newModels);
    }

    public async Task<int?> GetQuadClassAsync(string quadName)
    {
        var quad = await _quadModels.GetAll().FirstOrDefaultAsync(x => x.Name == quadName);
        return quad?.Class;
    }

    public async Task<bool> QuadsTheSameClassAsync(string firstQuad, string secondQuad)
    {
        var firstClass = await GetQuadClassAsync(firstQuad);
        var secondClass = await GetQuadClassAsync(secondQuad);

        return firstClass is not null && firstClass == secondClass;
    }
}
