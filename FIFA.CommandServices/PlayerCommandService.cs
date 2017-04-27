using FIFA.CommandServices.Interface;
using FIFA.Infrastructure;
using FIFA.Model;

namespace FIFA.CommandServices
{
    public class PlayerCommandService :
        IPlayerCommandService
    {
        private IRepository _repository;

        public PlayerCommandService(IRepository repository)
        {
            _repository = repository;
        }

        public void UpdatePlayer(UpdatePlayerCommand command)
        {
            using (var unitOfWork = UnitOfWorkFactory.CreateUnitOfWork())
            {
                var player = _repository.Load<Player>(command.PlayerId);

                if (!string.IsNullOrEmpty(command.SlackUsername))
                    player.SlackUsername = command.SlackUsername;

                if (!string.IsNullOrEmpty(command.Face))
                    player.Face = command.Face;

                if (!string.IsNullOrEmpty(command.Name))
                    player.Name = command.Name;

                _repository.Store(player);
            }
        }
    }
}
