using DBIntegrationHub.Application.Abstractions.Messaging;
using DBIntegrationHub.Application.Abstractions.Security;
using DBIntegrationHub.Domain.Entities;
using DBIntegrationHub.Domain.Repositories;
using DBIntegrationHub.Domain.Shared;

namespace DBIntegrationHub.Application.Connections.Commands.CreateConnection;

public class CreateConnectionCommandHandler : ICommandHandler<CreateConnectionCommand, Guid>
{
    private readonly IConnectionRepository _connectionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEncryptionService _encryptionService;

    public CreateConnectionCommandHandler(
        IConnectionRepository connectionRepository,
        IUnitOfWork unitOfWork,
        IEncryptionService encryptionService)
    {
        _connectionRepository = connectionRepository;
        _unitOfWork = unitOfWork;
        _encryptionService = encryptionService;
    }

    public async Task<Result<Guid>> Handle(CreateConnectionCommand request, CancellationToken cancellationToken)
    {
        // İsim kontrolü
        var existingConnection = await _connectionRepository.GetByNameAsync(request.Name, cancellationToken);
        if (existingConnection != null)
        {
            return Result.Failure<Guid>("Bu isimde bir bağlantı zaten mevcut.");
        }

        // Connection string'i şifrele
        var encryptedConnectionString = _encryptionService.Encrypt(request.ConnectionString);

        // Connection oluştur (şifrelenmiş connection string ile)
        var connection = Connection.Create(
            request.Name,
            encryptedConnectionString,
            request.DatabaseType);

        // Kaydet
        await _connectionRepository.AddAsync(connection, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(connection.Id);
    }
}

