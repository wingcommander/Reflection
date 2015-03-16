using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Web;
using System.Text;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Configuration;
using System.Reflection;
using System.IO;
using BBC.WW.BBCFood.BE;
using BBC.WW.BBCFood.BLL;

namespace BBC.WW.BBCFood.UI
{
	/// <summary>
	/// Summary description for ChefBiography.
	/// </summary>
	public class ChefBiography : pageBase
	{
		protected Wilson.WebControls.MasterPage MasterPage;
		protected System.Web.UI.WebControls.Literal litChefName;
		protected System.Web.UI.WebControls.Literal litChefCopy;
		protected System.Web.UI.WebControls.Image imgChefMain;
		protected System.Web.UI.WebControls.Literal litSubHeading;
		protected System.Web.UI.HtmlControls.HtmlAnchor linkBookSearch;
		protected System.Web.UI.HtmlControls.HtmlForm Form1;
		protected System.Web.UI.WebControls.Literal litFrmSearch;
		protected System.Web.UI.HtmlControls.HtmlAnchor linkSearchChef;
		protected System.Web.UI.WebControls.Literal litChefBooks;		
		protected StringBuilder inputs;

		protected int ZoneId
		{
			get
			{
				if(ViewState["zone"] == null)
				{
					ViewState.Add("zone",Request.QueryString["zone"]);
				}

				return Convert.ToInt32(ViewState["zone"]);
			}
		}

		public Guid ChefId
		{
			get
			{				
				if(ViewState["id"] == null)
				{
					ViewState.Add("id",Request.QueryString["id"]);
				}			

				return new Guid(ViewState["id"].ToString());
			}
				
		}

		public Guid RecipeGroup
		{
			get
			{
				return Guid.Empty;
			}			
		}

		public string Keywords
		{
			get
			{
				return string.Empty;
			}
		}

		public bool Vegetarian
		{
			get
			{
				return false;
			}
		}

		public bool Vegan
		{
			get
			{
				return false;
			}
		}
	
		private void Page_Load(object sender, System.EventArgs e)
		{
			if(!IsPostBack)
			{
				LoadChef();
				Page.DataBind();
			}

			HandleSearchChef();
		}		

		protected void LoadChef()
		{
			Chef chef = Chef.Create( this.ChefId, this.ZoneId );
			chef = Facade.RetrieveChef(chef);

			this.litChefName.Text = chef.ChefName;
			this.litChefCopy.Text = chef.Copy;
			this.litSubHeading.Text = chef.SubHeading;
			
			string ChefsImagesPath = ConfigurationSettings.AppSettings["ChefImagesPath"];
			string NoImage2Path = ConfigurationSettings.AppSettings["NoImage2Path"] as string + string.Empty;
			string mappedPath = Server.MapPath(ChefsImagesPath + @"/" + this.UserZone.ZoneId + @"/");

			if ( File.Exists(Path.Combine(mappedPath, chef.MainImage as string + string.Empty)) )
			{
				this.imgChefMain.ImageUrl = ChefsImagesPath + @"/" + this.UserCountry.ZoneId + @"/" + chef.MainImage;				
				this.imgChefMain.AlternateText = chef.ChefName;				
			}
			else
			{
				this.imgChefMain.ImageUrl = NoImage2Path;
				this.imgChefMain.AlternateText = "Image Not Found";
			}

			HandleBookLink(chef);
		}

		protected void HandleSearchChef()
		{
			if(Request.QueryString["action"] == "searchChef")
			{
				Server.Transfer("searchSummary.aspx");
			}
			else
			{
				linkSearchChef.HRef = "ChefBiography.aspx?id=" + ChefId.ToString() + "&zone=" + this.ZoneId + "&action=searchChef&menu=chef";
			}
		}

		protected void HandleBookLink(Chef chef)
		{
			if(this.UserCountry.ShopId != Guid.Empty)
			{
				this.litChefBooks.Visible = true;
				this.linkBookSearch.Visible = true;

				Shop shop = Facade.RetrieveShop( Shop.Create( this.UserCountry.ShopId ) );
				ShopParameterCollection shopParameters = Facade.RetrieveShopParameterCollection ( shop.Id );

				inputs = new StringBuilder();

				inputs.Append("<form method=\"" + shop.FormMethod + "\" action=\"{0}\" name=\"frmSearch\" id=\"frmSearch\" target=\"shopPopup\">\n");				

				foreach(ShopParameter shopParameter in shopParameters)
				{
					if(shopParameter.Vary == true)
					{						
						Type t = chef.GetType();
						PropertyInfo[] p = t.GetProperties();
						ArrayList properties = new ArrayList();
						foreach (PropertyInfo pi in p)
						{
							if(pi.Name.ToUpper() == shopParameter.Value.ToUpper())
							{								
								// If key value is not available e.g chef name or link id use alternative URL.								
								if(pi.GetValue(chef, null) == null)
								{
									inputs.Append("<input type=\"hidden\" value=\"\" id=\"" + shopParameter.Key.Trim() + "\" name=\"" + shopParameter.Key.Trim() + "\">\n");
									inputs.Replace("{0}",shop.AlternateURL);							

								}
								else
								{
									inputs.Append("<input type=\"hidden\" value=\"" + pi.GetValue(chef, null).ToString().Trim() + "\" id=\"" + shopParameter.Key.Trim() + "\" name=\"" + shopParameter.Key.Trim() + "\">\n");
									inputs.Replace("{0}",shop.URL);
								}

								break;
							}
						}
					}
					else
					{
						if(shopParameter.Value != null)
						{
							inputs.Append("<input type=\"hidden\" value=\"" + shopParameter.Value.Trim() + "\" id=\"" + shopParameter.Key.Trim() + "\" name=\"" + shopParameter.Key.Trim() + "\">\n");
						}
						else
						{
							inputs.Append("<input type=\"hidden\" value=\"\" id=\"" + shopParameter.Key.Trim() + "\" name=\"" + shopParameter.Key.Trim() + "\">\n");
						}

					}
				}

				inputs.Append("</form>\n");

				this.litFrmSearch.Text = inputs.ToString();				
			}
			else
			{
				this.litChefBooks.Visible = false;
				this.linkBookSearch.Visible = false;				
			}

		}

		#region Web Form Designer generated code
		override protected void OnInit(EventArgs e)
		{
			//
			// CODEGEN: This call is required by the ASP.NET Web Form Designer.
			//
			InitializeComponent();
			base.OnInit(e);
		}
		
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{    
			this.Load += new System.EventHandler(this.Page_Load);

		}		
				 
		#endregion
	}
}
