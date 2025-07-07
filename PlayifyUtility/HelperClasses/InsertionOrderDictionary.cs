using System.Collections;
using JetBrains.Annotations;
using PlayifyUtility.Utils.Extensions;
#if !NET48
using System.Diagnostics.CodeAnalysis;
#endif

namespace PlayifyUtility.HelperClasses;

[PublicAPI]
public class InsertionOrderDictionary<TKey,TValue>:IDictionary<TKey,TValue> where TKey : notnull{
	private readonly Dictionary<TKey,TValue> _dictionary;
	private readonly List<TKey> _order=[];

	public InsertionOrderDictionary()=>_dictionary=new Dictionary<TKey,TValue>();

	public InsertionOrderDictionary(IEqualityComparer<TKey> comparer)=>_dictionary=new Dictionary<TKey,TValue>(comparer);

	public InsertionOrderDictionary(IDictionary<TKey,TValue> dictionary){
		_dictionary=new Dictionary<TKey,TValue>(dictionary);
		_order.AddRange(dictionary.Keys);
	}

	public InsertionOrderDictionary(IDictionary<TKey,TValue> dictionary,IEqualityComparer<TKey> comparer){
		_dictionary=new Dictionary<TKey,TValue>(dictionary,comparer);
		_order.AddRange(dictionary.Keys);
	}

	#region Add
	public void Add(TKey key,TValue value){
		_dictionary.Add(key,value);//will throw if key already exists, therefore no _order.Remove is needed
		_order.Add(key);
	}

	public void Add(KeyValuePair<TKey,TValue> item)=>Add(item.Key,item.Value);
	#endregion

	#region Remove
	public bool Remove(TKey key){
		if(!_dictionary.Remove(key)) return false;
		_order.Remove(key);
		return true;
	}

	public bool Remove(KeyValuePair<TKey,TValue> item){
		if(!(_dictionary as IDictionary<TKey,TValue>).Remove(item)) return false;
		_order.Remove(item.Key);
		return true;
	}
	#endregion

	#region Get
#if NET48
	public bool TryGetValue(TKey key,out TValue value)=>_dictionary.TryGetValue(key,out value);
#else
	public bool TryGetValue(TKey key,[MaybeNullWhen(false)]out TValue value)=>_dictionary.TryGetValue(key,out value);
#endif

	public TValue this[TKey property]{
		get=>_dictionary[property];
		set{
			if(!_dictionary.ContainsKey(property))
				_order.Add(property);
			_dictionary[property]=value;
		}
	}
	public bool ContainsKey(TKey key)=>_dictionary.ContainsKey(key);
	public bool Contains(KeyValuePair<TKey,TValue> item)=>((IDictionary<TKey,TValue>)_dictionary).Contains(item);
	#endregion

	#region Special
	public void Clear(){
		_dictionary.Clear();
		_order.Clear();
	}

	public IEnumerator<KeyValuePair<TKey,TValue>> GetEnumerator()
		=>_order.Select(key=>new KeyValuePair<TKey,TValue>(key,_dictionary[key])).GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator()=>GetEnumerator();

	public void CopyTo(KeyValuePair<TKey,TValue>[] array,int arrayIndex){
		foreach(var key in _order)
			array[arrayIndex++]=new KeyValuePair<TKey,TValue>(key,_dictionary[key]);
	}

	public int Count=>_dictionary.Count;
	public bool IsReadOnly=>false;
	public ICollection<TKey> Keys=>_order.AsReadOnly();
	public ICollection<TValue> Values=>_order.Select(k=>_dictionary[k]).ToList().AsReadOnly();

	public bool EqualsIgnoreOrder(InsertionOrderDictionary<TKey,TValue>? other)
		=>other!=null&&other._dictionary.Count==_dictionary.Count&&
		  !other._dictionary.ToTuples().Except(_dictionary.ToTuples()).Any();

	public override bool Equals(object? obj)=>obj is InsertionOrderDictionary<TKey,TValue> other&&other._order.SequenceEqual(_order)&&EqualsIgnoreOrder(other);
	public override int GetHashCode()=>_dictionary.GetHashCode()^13*_order.GetHashCode();
	#endregion


}