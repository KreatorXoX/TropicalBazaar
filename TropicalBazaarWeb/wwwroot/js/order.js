var dataTable;

$(document).ready(function () {
    var url = window.location.search;

    if (url.includes("inprocess")) {
        loadDataTable("inprocess");
    }
    else if (url.includes("pending")) {
        loadDataTable("pending");
    }
    else if (url.includes("completed")) {
        loadDataTable("completed");
    }
    else if (url.includes("approved")) {
        loadDataTable("approved");
    }
    else
        loadDataTable("all");
    
});

function loadDataTable(status) {
    dataTable = $('#tblData').DataTable({
        
         
        "ajax": {
            "url":"/admin/order/GetAllOrders?status=" + status
        },
        "columns": [
            { "data": "id", "width": "5%" },
            { "data": "name", "width": "10%" },
            {"data":"phoneNumber","width":"15%"},
            { "data": "appuser.email", "width": "20%" },
            { "data": "orderStatus", "width": "20%" },
            {
                "data": "orderTotal", "render": function (data) {
                    return '$' + data.toFixed(2);
                },
                "width": "10%"
            },
            {
                "data": "id",
                "render": function (data) {
                    return `

                            <div class="d-flex justify-content-evenly">
						        <a href="/Admin/Order/Details?orderId=${data}"
                                    class="btn btn-warning"><i class="bi bi-file-earmark-break"></i></a>						    
						    </div>

                        `
                },
                "width":"15%"
            }
        ]
    });
}

