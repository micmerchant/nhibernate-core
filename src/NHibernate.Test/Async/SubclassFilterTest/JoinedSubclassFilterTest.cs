﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using System.Collections;
using NUnit.Framework;
using System.Linq;
using NHibernate.Linq;

namespace NHibernate.Test.SubclassFilterTest
{
	using System.Threading.Tasks;
	using System.Threading;
	[TestFixture]
	public class JoinedSubclassFilterTestAsync : TestCase
	{
		protected override string[] Mappings
		{
			get { return new string[] {"SubclassFilterTest.joined-subclass.hbm.xml"}; }
		}

		protected override string MappingsAssembly => "NHibernate.Test";

		[Test]
		public async Task FiltersWithSubclassAsync()
		{
			ISession s = OpenSession();
			s.EnableFilter("region").SetParameter("userRegion", "US");
			s.EnableFilter("minionsRegion").SetParameter("userRegion", "US");
			ITransaction t = s.BeginTransaction();

			await (PrepareTestDataAsync(s));

			IList results;

			results = await (s.CreateQuery("from Person").ListAsync());
			Assert.AreEqual(4, results.Count, "Incorrect qry result count");
			s.Clear();

			results = await (s.CreateQuery("from Employee").ListAsync());
			Assert.AreEqual(2, results.Count, "Incorrect qry result count");

			foreach (Person p in  results)
			{
				// find john
				if (p.Name.Equals("John Doe"))
				{
					Employee john = (Employee) p;
					Assert.AreEqual(1, john.Minions.Count, "Incorrect fecthed minions count");
					break;
				}
			}
			s.Clear();
			
			results = (await (s.CreateQuery("from Person as p left join fetch p.Minions").ListAsync<Person>())).Distinct().ToList();
			Assert.AreEqual(4, results.Count, "Incorrect qry result count");
			foreach (Person p in results)
			{
				if (p.Name.Equals("John Doe"))
				{
					Employee john = (Employee) p;
					Assert.AreEqual(1, john.Minions.Count, "Incorrect fecthed minions count");
					break;
				}
			}

			s.Clear();

			results = (await (s.CreateQuery("from Employee as p left join fetch p.Minions").ListAsync<Employee>())).Distinct().ToList();
			Assert.AreEqual(2, results.Count, "Incorrect qry result count");
			foreach (Person p in results)
			{
				if (p.Name.Equals("John Doe"))
				{
					Employee john = (Employee) p;
					Assert.AreEqual(1, john.Minions.Count, "Incorrect fecthed minions count");
					break;
				}
			}

			await (t.CommitAsync());
			s.Close();

			s = OpenSession();
			t = s.BeginTransaction();
			await (s.DeleteAsync("from Customer c where c.ContactOwner is not null"));
			await (s.DeleteAsync("from Employee e where e.Manager is not null"));
			await (s.DeleteAsync("from Person"));
			await (s.DeleteAsync("from Car"));
			await (t.CommitAsync());
			s.Close();
		}
		
		[Test]
		public async Task FilterCollectionWithSubclass1Async()
		{
			using var s = OpenSession();
			using var t = s.BeginTransaction();
			await (PrepareTestDataAsync(s));

			s.EnableFilter("minionsWithManager");

			var employees = await (s.Query<Employee>().Where(x => x.Minions.Any()).ToListAsync());
			Assert.That(employees.Count, Is.EqualTo(1));
			Assert.That(employees[0].Minions.Count, Is.EqualTo(2));
			
			await (t.RollbackAsync());
			s.Close();
		}

		[Test]
		public async Task FilterCollectionWithSubclass2Async()
		{
			using var s = OpenSession();
			using var t = s.BeginTransaction();
			await (PrepareTestDataAsync(s));

			s.EnableFilter("minionsRegion").SetParameter("userRegion", "US");

			var employees = await (s.Query<Employee>()
			                 .Where(x => x.Minions.Any())
			                 .ToListAsync());
			Assert.That(employees.Count, Is.EqualTo(1));
			Assert.That(employees[0].Minions.Count, Is.EqualTo(1));
			
			await (t.RollbackAsync());
			s.Close();
		}		
		
		[Test(Description = "Tests the joined subclass collection filter of a single table with a collection mapping " +
		                    "on the parent class.")]
		public async Task FilterCollectionWithSubclass3Async()
		{
			using ISession session = OpenSession();
			using ITransaction t = session.BeginTransaction();
			await (PrepareTestDataAsync(session));

			// sets the filter
			session.EnableFilter("region").SetParameter("userRegion", "US");

			var result = await (session.Query<Car>()
			                    .Where(c => c.Drivers.Any())
			                    .ToListAsync());
					
			Assert.AreEqual(1, result.Count);

			await (t.RollbackAsync());
			session.Close();
		}		
		
		private static async Task PrepareTestDataAsync(ISession session, CancellationToken cancellationToken = default(CancellationToken))
		{
			Car sharedCar1 = new Car { LicensePlate = "1234" };
			Car sharedCar2 = new Car { LicensePlate = "5678" };
			
			Employee john = new Employee("John Doe")
			                {
				                Company = "JBoss", 
				                Department = "hr", 
				                Title = "hr guru",
				                Region = "US",
				                SharedCar = sharedCar1
			                };

			Employee polli = new Employee("Polli Wog")
			                 {
				                 Company = "JBoss",
				                 Department = "hr",
				                 Title = "hr novice",
				                 Region = "US",
				                 Manager = john,
				                 SharedCar = sharedCar1
			                 };
			john.Minions.Add(polli);

			Employee suzie = new Employee("Suzie Q")
			                 {
				                 Company = "JBoss",
				                 Department = "hr", 
				                 Title = "hr novice", 
				                 Region = "EMEA",
				                 Manager = john,
				                 SharedCar = sharedCar2
			                 };
			john.Minions.Add(suzie);

			Customer cust = new Customer("John Q Public")
			                {
				                Company = "Acme", 
				                Region = "US", 
				                ContactOwner = john
			                };

			Person ups = new Person("UPS guy")
			             {
				             Company = "UPS", 
				             Region = "US"
			             };

			await (session.SaveAsync(sharedCar1, cancellationToken));
			await (session.SaveAsync(sharedCar2, cancellationToken));
			await (session.SaveAsync(john, cancellationToken));
			await (session.SaveAsync(cust, cancellationToken));
			await (session.SaveAsync(ups, cancellationToken));

			await (session.FlushAsync(cancellationToken));
			session.Clear();
		}
	}
}
