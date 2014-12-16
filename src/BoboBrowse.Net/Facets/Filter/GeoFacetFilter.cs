﻿// Version compatibility level: 3.1.0
namespace BoboBrowse.Net.Facets.Filter
{
    using BoboBrowse.Net.DocIdSet;
    using BoboBrowse.Net.Facets.Impl;
    using BoboBrowse.Net.Util;
    using Lucene.Net.Search;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// author nnarkhed
    /// </summary>
    public class GeoFacetFilter : RandomAccessFilter
    {
        private static long serialVersionUID = 1L;
	    private readonly FacetHandler<GeoFacetData> _handler;
	    private readonly float _lat;
	    private readonly float _lon;
        private readonly float _rad;
        // variable to specify if the geo distance calculations are in miles. Default is miles
        private bool _miles;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="facetHandler">The Geo Facet Handler for this instance</param>
        /// <param name="lat">latitude value of the user's point of interest</param>
        /// <param name="lon">longitude value of the user's point of interest</param>
        /// <param name="radius">Radius from the point of interest</param>
        /// <param name="miles">variable to specify if the geo distance calculations are in miles. False indicates distance calculation is in kilometers</param>
        public GeoFacetFilter(FacetHandler<GeoFacetData> facetHandler, float lat, float lon, float radius, bool miles)
        {
            _handler = facetHandler;
            _lat = lat;
            _lon = lon;
            _rad = radius;
            _miles = miles;
        }

        public override RandomAccessDocIdSet GetRandomAccessDocIdSet(BoboIndexReader reader)
        {
            int maxDoc = reader.MaxDoc;

		    GeoFacetData dataCache = _handler.GetFacetData(reader);
		    return new GeoDocIdSet(dataCache.get_xValArray(), dataCache.get_yValArray(), dataCache.get_zValArray(),
				_lat, _lon, _rad, maxDoc, _miles);
        }

        private sealed class GeoDocIdSet : RandomAccessDocIdSet
        {
            private readonly BigFloatArray _xvals;
		    private readonly BigFloatArray _yvals;
		    private readonly BigFloatArray _zvals;
		    private readonly float _radius;
		    private readonly float _targetX;
		    private readonly float _targetY;
		    private readonly float _targetZ;
            private readonly float _delta;
		    private readonly int _maxDoc;
	        // variable to specify if the geo distance calculations are in miles. Default is miles
	        private bool _miles;

            internal GeoDocIdSet(BigFloatArray xvals, BigFloatArray yvals, BigFloatArray zvals, float lat, float lon,
                float radius, int maxdoc, bool miles)
            {
                _xvals = xvals;
                _yvals = yvals;
                _zvals = zvals;
                _miles = miles;
                if (_miles)
                    _radius = GeoMatchUtil.GetMilesRadiusCosine(radius);
                else
                    _radius = GeoMatchUtil.GetKMRadiusCosine(radius);
                float[] coords = GeoMatchUtil.GeoMatchCoordsFromDegrees(lat, lon);
                _targetX = coords[0];
                _targetY = coords[1];
                _targetZ = coords[2];
                if (_miles)
                    _delta = (float)(radius / GeoMatchUtil.EARTH_RADIUS_MILES);
                else
                    _delta = (float)(radius / GeoMatchUtil.EARTH_RADIUS_KM);
                _maxDoc = maxdoc;
            }

            public override bool Get(int docId)
            {
                float docX = _xvals.Get(docId);
                float docY = _yvals.Get(docId);
                float docZ = _zvals.Get(docId);

                return InCircle(docX, docY, docZ, _targetX, _targetY, _targetZ, _radius);
            }

            public override DocIdSetIterator Iterator()
            {
                return new GeoDocIdSetIterator(_xvals, _yvals, _zvals, _targetX, _targetY, _targetZ, _delta, _radius, _maxDoc);
            }
        }

        private class GeoDocIdSetIterator : DocIdSetIterator
        {
            private readonly BigFloatArray _xvals;
            private readonly BigFloatArray _yvals;
            private readonly BigFloatArray _zvals;
            private readonly float _radius;
            private readonly float _targetX;
            private readonly float _targetY;
            private readonly float _targetZ;
            private readonly float _delta;
            private readonly int _maxDoc;
            private int _doc;

            internal GeoDocIdSetIterator(BigFloatArray xvals, BigFloatArray yvals, BigFloatArray zvals, float targetX, float targetY, float targetZ,
                float delta, float radiusCosine, int maxdoc)
            {
                _xvals = xvals;
                _yvals = yvals;
                _zvals = zvals;
                _targetX = targetX;
                _targetY = targetY;
                _targetZ = targetZ;
                _delta = delta;
                _radius = radiusCosine;
                _maxDoc = maxdoc;
                _doc = -1;
            }

            public sealed override int DocID()
            {
                return _doc;
            }

            public sealed override int NextDoc()
            {
                float x = _targetX;
                float xu = x + _delta;
                float xl = x - _delta;
                float y = _targetY;
                float yu = y + _delta;
                float yl = y - _delta;
                float z = _targetZ;
                float zu = z + _delta;
                float zl = z - _delta;

                int docid = _doc;
                while (docid < _maxDoc)
                {
                    docid++;

                    float docX = _xvals.Get(docid);
                    if (docX > xu || docX < xl) continue;

                    float docY = _yvals.Get(docid);
                    if (docY > yu || docY < yl) continue;

                    float docZ = _zvals.Get(docid);
                    if (docZ > zu || docZ < zl) continue;

                    if (GeoFacetFilter.InCircle(docX, docY, docZ, _targetX, _targetY, _targetZ, _radius))
                    {
                        _doc = docid;
                        return _doc;
                    }
                }
                _doc = DocIdSetIterator.NO_MORE_DOCS;
                return _doc;
            }

            public sealed override int Advance(int targetId)
            {
                if (_doc < targetId)
                    _doc = targetId - 1;

                float x = _targetX;
                float xu = x + _delta;
                float xl = x - _delta;
                float y = _targetY;
                float yu = y + _delta;
                float yl = y - _delta;
                float z = _targetZ;
                float zu = z + _delta;
                float zl = z - _delta;

                int docid = _doc;
                while (docid < _maxDoc)
                {
                    docid++;

                    float docX = _xvals.Get(docid);
                    if (docX > xu || docX < xl) continue;

                    float docY = _yvals.Get(docid);
                    if (docY > yu || docY < yl) continue;

                    float docZ = _zvals.Get(docid);
                    if (docZ > zu || docZ < zl) continue;

                    if (GeoFacetFilter.InCircle(docX, docY, docZ, _targetX, _targetY, _targetZ, _radius))
                    {
                        _doc = docid;
                        return _doc;
                    }
                }
                _doc = DocIdSetIterator.NO_MORE_DOCS;
                return _doc;
            }
        }

        public static bool InCircle(float docX, float docY, float docZ, float targetX, float targetY, float targetZ, float radCosine)
        {
            if (docX == -1.0f && docY == -1.0f && docZ == -1.0f)
                return false;
            float dotProductCosine = (docX * targetX) + (docY * targetY) + (docZ * targetZ);
            return (radCosine <= dotProductCosine);
        }
    }
}