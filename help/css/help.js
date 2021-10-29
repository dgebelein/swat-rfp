
//--------------------------------------------------------------------------------------
function ToggleVis( idDiv,idIconHide,idIconShow)  
{
	if (idDiv.style.display=="none") {
	 idDiv.style.display="";
	}
	else {
	 idDiv.style.display="none";
	}
	
	idIconHide.style.display = "none";
	idIconShow.style.display = "";

}
//--------------------------------------------------------------------------------------
