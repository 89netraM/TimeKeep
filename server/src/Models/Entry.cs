using System;
using System.Collections.Generic;

#pragma warning disable CS8618

namespace TimeKeep.Models;

public class Entry
{
	public Guid Id { get; set; }
	public DateTime Start { get; set; }
	public DateTime? End { get; set; }
	public ICollection<Category> Categories { get; set; }
	public Location? Location { get; set; }
}
