var dataTable;

$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#compTable').DataTable({
         
        "ajax": {
            "url":"/Admin/Company/GetCompanies"
        },
        "columns": [
            { "data": "name", "width": "15%" },
            { "data": "address", "width": "20%" },
            {"data":"city","width":"10%"},
            { "data": "state", "width": "5%" },
            { "data": "phoneNumber", "width": "15%" },
            {
                "data": "id",
                "render": function (data) {
                    return `

                            <div class="d-flex justify-content-around">
						        <a href="/Admin/Company/Upsert?id=${data}" class="btn btn-sm btn-warning"><i class="bi bi-file-earmark-break"></i>&nbsp; Edit</a>
						        <a onClick = Delete('/Admin/Company/Delete/${data}')
                                class="btn btn-sm btn-danger"><i class="bi bi-folder-x"></i>&nbsp; Delete</a>
						    </div>

                        `
                },
                "width":"35%"
            }
        ]
    });
}


function Delete(url) {
    Swal.fire({
        title: 'Are you sure?',
        text: "You won't be able to revert this!",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Yes, delete it!'
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: url,
                type: 'DELETE',
                success: function (data) {
                    if (data.success) {
                        dataTable.ajax.reload();
                        toastr.success(data.message);
                    }
                    else {
                        toastr.error(data.message);
                    }
                }
            })
        }
    })
}
