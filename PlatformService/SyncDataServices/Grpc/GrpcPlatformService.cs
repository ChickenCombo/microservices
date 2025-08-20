using AutoMapper;
using Grpc.Core;
using PlatformService.Interfaces;

namespace PlatformService.SyncDataServices.Grpc;

public class GrpcPlatformService(IPlatformRepository repository, IMapper mapper) : GrpcPlatform.GrpcPlatformBase
{
    private readonly IPlatformRepository _platformRepository = repository;
    private readonly IMapper _mapper = mapper;

    public override Task<PlatformResponse> GetAllPlatforms(GetAllRequest request, ServerCallContext context)
    {
        var response = new PlatformResponse();
        var platforms = _platformRepository.GetAllPlatforms();

        foreach (var platform in platforms)
        {
            var platformDto = _mapper.Map<GrpcPlatformModel>(platform);
            response.Platform.Add(platformDto);
        }

        return Task.FromResult(response);
    }
}