﻿using System;
using System.Collections.Generic;
using FaithEngage.Core.Events.EventSchedules;
using FaithEngage.Core.Events.EventSchedules.Interfaces;
using FaithEngage.Core.Factories;
using FaithEngage.Core.RepoInterfaces;

namespace FaithEngage.Core.RepoManagers
{
	//TODO: Flesh this out
	public class EventScheduleRepoManager : IEventScheduleRepoManager
	{
        public EventScheduleRepoManager (IEventScheduleRepository _repo, IConverterFactory<EventSchedule,EventScheduleDTO> dtoFactory, IConverterFactory<EventScheduleDTO, EventSchedule> factory)
        {
            throw new NotImplementedException ();
        }

        public void DeleteSchedule(Guid id)
		{
			throw new NotImplementedException();
		}

		public EventSchedule GetById(Guid id)
		{
			throw new NotImplementedException();
		}

		public IList<EventSchedule> GetByOrgId(Guid id)
		{
			throw new NotImplementedException();
		}

		public Guid SaveSchedule(EventSchedule schedule)
		{
			throw new NotImplementedException();
		}

		public void UpdateSchedule(EventSchedule schedule)
		{
			throw new NotImplementedException();
		}
	}
}

