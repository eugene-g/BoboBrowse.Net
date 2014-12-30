﻿// Version compatibility level: 3.1.0
namespace BoboBrowse.Net.Facets.Data
{
    using System;

    public interface IFacetDataFetcher
    {
        object Fetch(BoboIndexReader reader, int doc);
        void Cleanup(BoboIndexReader reader);
    }
}