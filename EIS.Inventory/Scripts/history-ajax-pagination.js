// Adding popstate event listener to handle browser back button  
window.addEventListener("popstate", function (e) {
    doAjaxRequest(location.href);
});

$(document).ready(function () {
    $("body").on("click", "#pagedListPager .pagination a", function (event) {
        event.preventDefault();
        var url = $(this).attr("href");
        if (!url)
            return false;

        doAjaxRequest(url);
        changeUrl('index', url);
    })
});

function changeUrl(pageName, url) {
    if (typeof (history.pushState) != undefined) {
        var obj = { Page: pageName, Url: url };
        history.pushState(null, obj.Page, obj.Url);
    } else {
        alert("Browser does not support HTML5.");
    }
}

function doAjaxRequest(url) {   
    $.ajax({
        url: url,
        beforeSend: function () {
            $("#loadingDiv").show();
        },
        success: function (result) {
            $('#model_list_container').html(result);
        },
        complete: function () {
            // call the post action for the ajax - this should be implemented on each page
            loadPagedDataComplete();
            $("#loadingDiv").hide();
        }
    });
}