﻿// Version compatibility level: 3.1.0
namespace BoboBrowse.Net.Facets.Impl
{
    using BoboBrowse.Net.Facets.Data;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// author "Xiaoyang Gu<xgu@linkedin.com>"
    /// </summary>
    public class DefaultShortFacetIterator : ShortFacetIterator
    {
        public TermShortList _valList;
        private int[] _count;
        private int _countlength;
        private int _countLengthMinusOne;
        private int _index;

        public DefaultShortFacetIterator(TermShortList valList, int[] countarray, int countlength, bool zeroBased)
        {
            _valList = valList;
            _countlength = countlength;
            _count = countarray;
            _countLengthMinusOne = _countlength - 1;
            _index = -1;
            if (!zeroBased)
                _index++;
            _facet = TermShortList.VALUE_MISSING;
            base._count = 0;
        }

        /// <summary>
        /// (non-Javadoc)
        /// see com.browseengine.bobo.api.FacetIterator#getFacet()
        /// </summary>
        public virtual string Facet
        {
            get
            {
                if (_facet == TermShortList.VALUE_MISSING) return null;
                return _valList.Format(_facet);
            }
        }

        public override string Format(short val)
        {
            return _valList.Format(val);
        }

        public override string Format(object val)
        {
            return _valList.Format(val);
        }

        /// <summary>
        /// (non-Javadoc)
        /// see com.browseengine.bobo.api.FacetIterator#getFacetCount()
        /// </summary>
        public virtual int FacetCount
        {
            get { return base._count; }
        }

        /// <summary>
        /// (non-Javadoc)
        /// see java.util.Iterator#hasNext()
        /// </summary>
        /// <returns></returns>
        public virtual bool HasNext()
        {
            return (_index < _countLengthMinusOne);
        }

        /// <summary>
        /// (non-Javadoc)
        /// see java.util.Iterator#next()
        /// </summary>
        /// <returns></returns>
        public override string Next()
        {
            if ((_index >= 0) && (_index >= _countLengthMinusOne))
                throw new IndexOutOfRangeException("No more facets in this iteration");
            _index++;
            _facet = _valList.GetPrimitiveValue(_index);
            base._count = _count[_index];
            return _valList.Get(_index);
        }

        /// <summary>
        /// (non-Javadoc)
        /// see com.browseengine.bobo.api.ShortFacetIterator#nextShort()
        /// </summary>
        /// <returns></returns>
        public override short NextShort()
        {
            if (_index >= _countLengthMinusOne)
                throw new IndexOutOfRangeException("No more facets in this iteration");
            _index++;
            _facet = _valList.GetPrimitiveValue(_index);
            base._count = _count[_index];
            return _facet;
        }

        /// <summary>
        /// (non-Javadoc)
        /// see java.util.Iterator#remove()
        /// </summary>
        public virtual void Remove()
        {
            throw new NotSupportedException("remove() method not supported for Facet Iterators");
        }

        /// <summary>
        /// (non-Javadoc)
        /// see com.browseengine.bobo.api.FacetIterator#next(int)
        /// </summary>
        /// <param name="minHits"></param>
        /// <returns></returns>
        public override string Next(int minHits)
        {
            while (++_index < _countlength)
            {
                if (_count[_index] >= minHits)
                {
                    _facet = _valList.GetPrimitiveValue(_index);
                    base._count = _count[_index];
                    return _valList.Format(_facet);
                }
            }
            _facet = TermShortList.VALUE_MISSING;
            base._count = 0;
            return null;
        }
        
        /// <summary>
        /// (non-Javadoc)
        /// see com.browseengine.bobo.api.ShortFacetIterator#nextShort(int)
        /// </summary>
        /// <param name="minHits"></param>
        /// <returns></returns>
        public override short NextShort(int minHits)
        {
            while (++_index < _countlength)
            {
                if (_count[_index] >= minHits)
                {
                    _facet = _valList.GetPrimitiveValue(_index);
                    base._count = _count[_index];
                    return _facet;
                }
            }
            _facet = TermShortList.VALUE_MISSING;
            base._count = 0;
            return _facet;
        }
    }
}