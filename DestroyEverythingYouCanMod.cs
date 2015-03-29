namespace DestroyEverythingYouCan
{
	using ICities;

	public class DestroyEverythingYouCanMod: IUserMod
	{
		public string Name
		{
			get { return "Destroy everything you can"; }
		}

		public string Description
		{
			get { return "Shows you a list of your currently available buildings and structures and allows you to burn and/or destroy all that buildings at once."; }
		}
	}
}

