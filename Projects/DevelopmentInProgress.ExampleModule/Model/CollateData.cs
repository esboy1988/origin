﻿using System;
using System.Linq;
using System.Threading.Tasks;
using DevelopmentInProgress.DipState;
using DevelopmentInProgress.ExampleModule.Service;

namespace DevelopmentInProgress.ExampleModule.Model
{
    public class CollateData : EntityBase
    {
        private decimal? nominalAmount;
        private decimal? interest;
        private DateTime? startDate;
        private decimal? redressRate;

        public CollateData()
        {
            this.AddCanCompletePredicateAsync(HasRedressRateAsync);
            this.AddActionAsync(StateActionType.OnStatusChanged, RefreshAsync);
        }

        public string HedgingProduct { get; set; }

        public decimal? RedressAmount
        {
            get
            {
                decimal? redressAmount = null;
                if (NominalAmount.HasValue
                    && Interest.HasValue)
                {
                    redressAmount = nominalAmount*(interest/100);
                }

                return redressAmount;
            }
        }

        public decimal? NominalAmount
        {
            get { return nominalAmount; }
            set
            {
                nominalAmount = value;
                OnPropertyChanged(String.Empty);
            }
        }

        public decimal? Interest
        {
            get { return interest; }
            set
            {
                interest = value;
                OnPropertyChanged(String.Empty);
            }
        }

        public DateTime? StartDate
        {
            get { return startDate; }
            set
            {
                startDate = value;
                OnPropertyChanged(String.Empty);
            }
        }

        public decimal? RedressRate
        {
            get { return redressRate; }
            set
            {
                redressRate = value;
                OnPropertyChanged(String.Empty);
            }
        }

        private async Task<bool> HasRedressRateAsync(State state)
        {
            if (((CollateData) state).RedressAmount.HasValue)
            {
                if (Transition == null)
                {
                    Transition = Transitions.FirstOrDefault();
                }

                await TaskRunner.DoAsyncStuff();

                return true;
            }

            var error = String.Format("{0} requires a redress amount before it can be completed.", state.Name);
            state.WriteLogEntry(error);
            throw new StateException(state, error);
        }
    }
}
