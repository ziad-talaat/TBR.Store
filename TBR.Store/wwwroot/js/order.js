$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        ajax: { url: '/admin/order/GetAll'},
        "columns": [
            { data: 'id',"width":"25%"},
            { data: 'name', "width": "15%" },
            { data: 'phoneNumber', "width": "10%"},
            { data: 'user.email', "width": "20%" },
            { data: 'orderStatus', "width": "15%" },
            { data: 'orderTotal', "width": "10%" },  
            {

             data: 'id',
                "render": function (data) {
                    return `<div class="w-75 btn-group" role="group">
                <a href="/admin/order/details?id=${data}" class="btn btn-primary mx-2"> <i class="bi bi-pencil-square"></i></a>
            </div>`
                },
                "width": "10%"
            }

            
        ]
    });
}
                   