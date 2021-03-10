using Radial.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Radial.Services
{
    public interface IPartyService
    {
        void CreateNewParty(PlayerCharacter leader);
    }
    public class PartyService : IPartyService
    {
        public void CreateNewParty(PlayerCharacter leader)
        {
            leader.Party = new Party(leader);
            
        }
    }
}
