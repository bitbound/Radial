using Radial.Models;
using Radial.Models.Messaging;
using Radial.Services.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Radial.Services
{
    public interface IPartyService
    {
        void AcceptInvite(IClientConnection client, Party party);
        void DeclineInvite(IClientConnection client, Party party);
        void CreateNewParty(PlayerCharacter leader);
        void SendInvite(IClientConnection leader, string inviteeName);
    }
    public class PartyService : IPartyService
    {
        private readonly IToastService _toastService;
        private readonly IClientManager _clientManager;

        public PartyService(IToastService toastService, IClientManager clientManager)
        {
            _toastService = toastService;
            _clientManager = clientManager;
        }

        public void AcceptInvite(IClientConnection client, Party party)
        {
            var leader = _clientManager.Clients.FirstOrDefault(x => x.Character == party.Leader);

            if (leader is null)
            {
                _toastService.ShowToast("Party not found.", classString: "bg-warning");
                return;
            }

            client.Character.PartyInvites.Remove(party);

            if (client.Character.Party is not null)
            {
                client.Character.Party.Members.RemoveAll(x => x.Name == client.Character.Name);
            }

            party.Members.RemoveAll(x => x.Name == client.Character.Name);
            client.Character.Party = party;
            party.Members.Add(client.Character);
        }

        public void CreateNewParty(PlayerCharacter leader)
        {
            leader.Party = new Party(leader);
            
        }

        public void DeclineInvite(IClientConnection client, Party party)
        {
            var leader = _clientManager.Clients.FirstOrDefault(x => x.Character == party.Leader);

            if (leader is not null)
            {
                leader.InvokeMessageReceived(new ToastMessage($"{client.Character.Name} declined the party invite.", classString: "bg-warning"));
            }

            client.Character.PartyInvites.Remove(party);
        }

        public void SendInvite(IClientConnection leader, string inviteeName)
        {
            var invitee = _clientManager.Clients.FirstOrDefault(x => x.Character.Name.Equals(inviteeName?.Trim(), StringComparison.OrdinalIgnoreCase));

            if (invitee is null)
            {
                _toastService.ShowToast("User not found.", classString: "bg-warning");
                return;
            }

            invitee.Character.PartyInvites.Add(leader.Character.Party);
            invitee.InvokeMessageReceived(new PartyInvite(leader.Character));

            _toastService.ShowToast("Invite sent.", classString: "bg-success");
        }
    }
}
